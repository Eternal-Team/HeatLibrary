using BaseLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace HeatLibrary
{
	public class HeatHandler
	{
		public long Heat { get; private set; }

		// note: should this have capacity?
		public long Capacity { get; private set; }
		public long MaxExtract { get; private set; }
		public long MaxReceive { get; private set; }
		public long Dissapation { get; private set; }

		public long CurrentDelta { get; private set; }
		public long AverageDelta { get; private set; }

		private Queue<long> DeltaBuffer = new Queue<long>();

		public Action OnChanged = () => { };

		public HeatHandler()
		{
		}

		public HeatHandler(long capacity)
		{
			Capacity = capacity;
			MaxReceive = capacity;
			MaxExtract = capacity;
		}

		public HeatHandler(long capacity, long maxTransfer)
		{
			Capacity = capacity;
			MaxReceive = maxTransfer;
			MaxExtract = maxTransfer;
		}

		public HeatHandler(long capacity, long maxReceive, long maxExtract)
		{
			Capacity = capacity;
			MaxReceive = maxReceive;
			MaxExtract = maxExtract;
		}

		public HeatHandler Clone() => new HeatHandler
		{
			Heat = Heat,
			Capacity = Capacity,
			MaxExtract = MaxExtract,
			MaxReceive = MaxReceive,
			OnChanged = (Action)OnChanged.Clone()
		};

		public void SetCapacity(long capacity)
		{
			Capacity = capacity;
			if (Heat > capacity) Heat = Capacity;

			OnChanged?.Invoke();
		}

		public void AddCapacity(long capacity)
		{
			Capacity += capacity;
			if (Heat > Capacity) Heat = Capacity;

			OnChanged?.Invoke();
		}

		public void SetMaxTransfer(long maxTransfer)
		{
			SetMaxReceive(maxTransfer);
			SetMaxExtract(maxTransfer);

			OnChanged?.Invoke();
		}

		public void SetMaxReceive(long maxReceive)
		{
			MaxReceive = maxReceive;

			OnChanged?.Invoke();
		}

		public void SetMaxExtract(long maxExtract)
		{
			MaxExtract = maxExtract;

			OnChanged?.Invoke();
		}

		public void ModifyHeat(IHeatHandler heatHandler)
		{
			HeatHandler handler = heatHandler.HeatHandler;

			if (handler.Heat > Heat)
			{
				long extracted = handler.ExtractEnergy(Math.Min(Capacity - Heat, MaxReceive));
				InsertEnergy(-extracted);
			}
			else
			{
				long extracted = ExtractEnergy(Math.Min(handler.Capacity - handler.Heat, handler.MaxReceive));
				handler.InsertEnergy(-extracted);
			}
		}

		private long InsertEnergy(long amount)
		{
			CurrentDelta = Utility.Min(Capacity - Heat, MaxReceive, amount);
			Heat += CurrentDelta;

			DeltaBuffer.Enqueue(CurrentDelta);

			if (DeltaBuffer.Count > HeatLibrary.Instance.GetConfig<HeatLibraryConfig>().DeltaCacheSize)
			{
				DeltaBuffer.Dequeue();
				AverageDelta = (long)DeltaBuffer.Average(i => i);
			}
			else AverageDelta = CurrentDelta;

			OnChanged?.Invoke();

			return CurrentDelta;
		}

		private long ExtractEnergy(long amount)
		{
			CurrentDelta = -Utility.Min(Heat, MaxExtract, amount);
			Heat += CurrentDelta;

			DeltaBuffer.Enqueue(CurrentDelta);

			if (DeltaBuffer.Count > HeatLibrary.Instance.GetConfig<HeatLibraryConfig>().DeltaCacheSize)
			{
				DeltaBuffer.Dequeue();
				AverageDelta = (long)DeltaBuffer.Average(i => i);
			}
			else AverageDelta = CurrentDelta;

			OnChanged?.Invoke();

			return CurrentDelta;
		}

		public void Update()
		{
			ExtractEnergy(Dissapation);
		}

		public TagCompound Save() => new TagCompound
		{
			["Heat"] = Heat,
			["Capacity"] = Capacity,
			["MaxExtract"] = MaxExtract,
			["MaxReceive"] = MaxReceive,
			["Dissapation"] = Dissapation
		};

		public void Load(TagCompound tag)
		{
			Heat = tag.GetLong("Heat");
			Capacity = tag.GetLong("Capacity");
			MaxExtract = tag.GetLong("MaxExtract");
			MaxReceive = tag.GetLong("MaxReceive");
			Dissapation = tag.GetLong("Dissapation");
		}

		// bug: delta doesn't get sent over (but it also is a lot of data so I might just need to send extract/insert events and have it calculated on client/server)
		public void Write(BinaryWriter writer)
		{
			writer.Write(Heat);
			writer.Write(Capacity);
			writer.Write(MaxExtract);
			writer.Write(MaxReceive);
			writer.Write(Dissapation);
		}

		public void Read(BinaryReader reader)
		{
			Heat = reader.ReadInt64();
			Capacity = reader.ReadInt64();
			MaxExtract = reader.ReadInt64();
			MaxReceive = reader.ReadInt64();
			Dissapation = reader.ReadInt64();
		}

		public override string ToString() => $"Heat: {Heat.ToSI("N1")}/{Capacity.ToSI("N1")}J MaxExtract: {MaxExtract} MaxReceive: {MaxReceive}";
	}
}