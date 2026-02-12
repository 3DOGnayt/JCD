using Scellecs.Morpeh;
using UnityEngine;

namespace Services
{
    public enum GameSessionTarget
    {
        Menu,
        Game
    }

    public interface IGameSessionService
    {
        GameSessionTarget Target { get; }

        void BeginGame();
        void RestartGame();
        void ExitToMenu();

        void RegisterRuntimeRoot(GameObject instance);
        void RegisterRuntimeEntity(Entity entity);
    }
}
