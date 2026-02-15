using Data.Enums;
using Scellecs.Morpeh;
using UnityEngine;

namespace Services
{
    public interface IGameSessionService
    {
        EGameSessionTarget Target { get; }

        void BeginGame();
        void RestartGame();
        void ExitToMenu();

        void RegisterRuntimeRoot(GameObject instance);
        void RegisterRuntimeEntity(Entity entity);
    }
}