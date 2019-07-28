﻿using Core.Models.Serialization;
using Newtonsoft.Json;

namespace Core.Models.Events
{
	public class EntryHoldKillEvent : BaseEvent
	{
		[JsonProperty("round_number")]
		public int RoundNumber { get; set; }

		[JsonProperty("killer_steamid")]
		[JsonConverter(typeof(LongToStringConverter))]
		public long KillerSteamId { get; set; }

		[JsonProperty("killer_name")]
		public string KillerName { get; set; }

		[JsonProperty("killer_side")]
		[JsonConverter(typeof(SideToStringConverter))]
		public Side KillerSide { get; set; }

		[JsonProperty("killed_steamid")]
		[JsonConverter(typeof(LongToStringConverter))]
		public long KilledSteamId { get; set; }

		[JsonProperty("killed_name")]
		public string KilledName { get; set; }

		[JsonProperty("killed_side")]
		[JsonConverter(typeof(SideToStringConverter))]
		public Side KilledSide { get; set; }

		[JsonProperty("weapon")]
		public Weapon Weapon { get; set; }

		[JsonProperty("has_won")]
		public bool HasWon { get; set; }

		[JsonProperty("has_won_round")]
		public bool HasWonRound { get; set; }

		public EntryHoldKillEvent(int tick, float seconds) : base(tick, seconds) { }

		public EntryHoldKillEvent Clone()
		{
			return (EntryHoldKillEvent)MemberwiseClone();
		}
	}
}
