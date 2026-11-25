using System;
using System.IO;
using System.Linq;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Localiza y extrae, dentro del AFS "unnamed_47.afs" (contenido en
    /// GAME.afs del disco del juego), el archivo .adx cuyo nombre coincide con
    /// un código de callname concreto (ej. "R12082" -&gt; "VP_P0_R12082.adx").
    /// </summary>
    public static class CallnameAudioLocator
    {
        /// <summary>
        /// Busca, dentro de <paramref name="unnamed47Archive"/>, la entrada de
        /// audio cuyo nombre contenga el código de callname indicado (ej.
        /// "R12082"), siguiendo el patrón "VP_P0_{código}.adx" usado por el
        /// juego. Devuelve null si no se encuentra ninguna coincidencia.
        /// </summary>
        public static AfsEntry? FindAudioEntry(AfsArchive unnamed47Archive, string callnameCode)
        {
            if (unnamed47Archive == null || string.IsNullOrWhiteSpace(callnameCode))
            {
                return null;
            }

            string normalizedCode = callnameCode.Trim();

            // Coincidencia exacta esperada: "VP_P0_{código}.adx"
            string expectedName = $"VP_P0_{normalizedCode}.adx";

            AfsEntry? match = unnamed47Archive.Entries.FirstOrDefault(e =>
                string.Equals(e.Name, expectedName, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return match;
            }

            // Respaldo: cualquier entrada cuyo nombre contenga el código como
            // segmento delimitado (evita falsos positivos como "R1" dentro de "R12").
            return unnamed47Archive.Entries.FirstOrDefault(e =>
                e.Name.EndsWith($"{normalizedCode}.adx", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Extrae los bytes crudos del .adx correspondiente al código de
        /// callname indicado, o null si no se encuentra.
        /// </summary>
        public static byte[]? ExtractAudio(AfsArchive unnamed47Archive, string callnameCode)
        {
            AfsEntry? entry = FindAudioEntry(unnamed47Archive, callnameCode);
            return entry == null ? null : AfsArchive.ExtractEntry(entry);
        }
    }
}