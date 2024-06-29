﻿using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Patron;

[Serializable, NetSerializable]
public record SharedRMCPatronTier(
    bool ShowOnCredits,
    bool NamedItems,
    bool Figurines,
    bool LobbyMessage,
    bool RoundEndShoutout,
    string Tier
);
