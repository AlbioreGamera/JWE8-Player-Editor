using System.Collections.Generic;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Dataset estático de jugadores reales asociados a cada código de cara
    /// preestablecida (W/Y/R/K). Pendiente de poblar con el listado completo.
    /// </summary>
    public static class LoadFacePlayers
    {
        public static List<FacePlayer> FacePlayers { get; } = new List<FacePlayer>();
    }
}