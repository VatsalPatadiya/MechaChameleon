using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MechaChameleon.Core
{
    public enum PlayerRole { Unassigned, Hider, Hunter }

    /// <summary>
    /// Serialisable snapshot of one player's state, synced as a NetworkVariable.
    /// </summary>
    [Serializable]
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong ClientId;
        public FixedString64Bytes DisplayName;
        public PlayerRole Role;
        public int Score;
        public bool IsReady;
        public bool IsCaught;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref DisplayName);
            serializer.SerializeValue(ref Role);
            serializer.SerializeValue(ref Score);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsCaught);
        }

        public bool Equals(PlayerData other) =>
            ClientId == other.ClientId &&
            DisplayName == other.DisplayName &&
            Role == other.Role &&
            Score == other.Score &&
            IsReady == other.IsReady &&
            IsCaught == other.IsCaught;
    }
}
