using PESPlayerEditorTest;
using PlayerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWE8Editor.Logic
{
    public class LoadCountries
    {
        public static List<Country> Countries { get; } = new List<Country>();

        public LoadCountries()
        {
            PopulateCommentary();
        }

        public void PopulateCommentary()
        {
            Countries.Add(new Country { CountryIndex = 0, CountryId = 0, CountryName = "Austria" });
            Countries.Add(new Country { CountryIndex = 1, CountryId = 1, CountryName = "Belgium" });
            Countries.Add(new Country { CountryIndex = 2, CountryId = 2, CountryName = "Bulgaria" });
            Countries.Add(new Country { CountryIndex = 3, CountryId = 3, CountryName = "Croatia" });
            Countries.Add(new Country { CountryIndex = 4, CountryId = 4, CountryName = "Czech" });
            Countries.Add(new Country { CountryIndex = 5, CountryId = 5, CountryName = "Denmark" });
            Countries.Add(new Country { CountryIndex = 6, CountryId = 6, CountryName = "England" });
            Countries.Add(new Country { CountryIndex = 7, CountryId = 7, CountryName = "Finland" });
            Countries.Add(new Country { CountryIndex = 8, CountryId = 8, CountryName = "France" });
            Countries.Add(new Country { CountryIndex = 9, CountryId = 9, CountryName = "Germany" });
            Countries.Add(new Country { CountryIndex = 10, CountryId = 10, CountryName = "Greece" });
            Countries.Add(new Country { CountryIndex = 11, CountryId = 11, CountryName = "Hungary" });
            Countries.Add(new Country { CountryIndex = 12, CountryId = 12, CountryName = "Ireland" });
            Countries.Add(new Country { CountryIndex = 13, CountryId = 13, CountryName = "Italy" });
            Countries.Add(new Country { CountryIndex = 14, CountryId = 14, CountryName = "Latvia" });
            Countries.Add(new Country { CountryIndex = 15, CountryId = 15, CountryName = "Netherlands" });
            Countries.Add(new Country { CountryIndex = 16, CountryId = 16, CountryName = "Northern Ireland" });
            Countries.Add(new Country { CountryIndex = 17, CountryId = 17, CountryName = "Norway" });
            Countries.Add(new Country { CountryIndex = 18, CountryId = 18, CountryName = "Poland" });
            Countries.Add(new Country { CountryIndex = 19, CountryId = 19, CountryName = "Portugal" });
            Countries.Add(new Country { CountryIndex = 20, CountryId = 20, CountryName = "Romania" });
            Countries.Add(new Country { CountryIndex = 21, CountryId = 21, CountryName = "Russia" });
            Countries.Add(new Country { CountryIndex = 22, CountryId = 22, CountryName = "Scotland" });
            Countries.Add(new Country { CountryIndex = 23, CountryId = 23, CountryName = "Serbia and Montenegro" });
            Countries.Add(new Country { CountryIndex = 24, CountryId = 24, CountryName = "Slovakia" });
            Countries.Add(new Country { CountryIndex = 25, CountryId = 25, CountryName = "Slovenia" });
            Countries.Add(new Country { CountryIndex = 26, CountryId = 26, CountryName = "Spain" });
            Countries.Add(new Country { CountryIndex = 27, CountryId = 27, CountryName = "Sweden" });
            Countries.Add(new Country { CountryIndex = 28, CountryId = 28, CountryName = "Switzerland" });
            Countries.Add(new Country { CountryIndex = 29, CountryId = 29, CountryName = "Turkey" });
            Countries.Add(new Country { CountryIndex = 30, CountryId = 30, CountryName = "Ukraine" });
            Countries.Add(new Country { CountryIndex = 31, CountryId = 31, CountryName = "Wales" });
            Countries.Add(new Country { CountryIndex = 32, CountryId = 32, CountryName = "Angola" });
            Countries.Add(new Country { CountryIndex = 33, CountryId = 33, CountryName = "Cameroon" });
            Countries.Add(new Country { CountryIndex = 34, CountryId = 34, CountryName = "Cote d'Ivoire" });
            Countries.Add(new Country { CountryIndex = 35, CountryId = 35, CountryName = "Ghana" });
            Countries.Add(new Country { CountryIndex = 36, CountryId = 36, CountryName = "Nigeria" });
            Countries.Add(new Country { CountryIndex = 37, CountryId = 37, CountryName = "South Africa" });
            Countries.Add(new Country { CountryIndex = 38, CountryId = 38, CountryName = "Togo" });
            Countries.Add(new Country { CountryIndex = 39, CountryId = 39, CountryName = "Tunisia" });
            Countries.Add(new Country { CountryIndex = 40, CountryId = 40, CountryName = "Costa Rica" });
            Countries.Add(new Country { CountryIndex = 41, CountryId = 41, CountryName = "Mexico" });
            Countries.Add(new Country { CountryIndex = 42, CountryId = 42, CountryName = "Trinidad and Tobago" });
            Countries.Add(new Country { CountryIndex = 43, CountryId = 43, CountryName = "United States" });
            Countries.Add(new Country { CountryIndex = 44, CountryId = 44, CountryName = "Argentina" });
            Countries.Add(new Country { CountryIndex = 45, CountryId = 45, CountryName = "Brazil" });
            Countries.Add(new Country { CountryIndex = 46, CountryId = 46, CountryName = "Chile" });
            Countries.Add(new Country { CountryIndex = 47, CountryId = 47, CountryName = "Colombia" });
            Countries.Add(new Country { CountryIndex = 48, CountryId = 48, CountryName = "Ecuador" });
            Countries.Add(new Country { CountryIndex = 49, CountryId = 49, CountryName = "Paraguay" });
            Countries.Add(new Country { CountryIndex = 50, CountryId = 50, CountryName = "Peru" });
            Countries.Add(new Country { CountryIndex = 51, CountryId = 51, CountryName = "Uruguay" });
            Countries.Add(new Country { CountryIndex = 52, CountryId = 52, CountryName = "Iran" });
            Countries.Add(new Country { CountryIndex = 53, CountryId = 53, CountryName = "Japan" });
            Countries.Add(new Country { CountryIndex = 54, CountryId = 54, CountryName = "Saudi Arabia" });
            Countries.Add(new Country { CountryIndex = 55, CountryId = 55, CountryName = "South Korea" });
            Countries.Add(new Country { CountryIndex = 56, CountryId = 56, CountryName = "Australia" });
            Countries.Add(new Country { CountryIndex = 57, CountryId = 57, CountryName = "Bosnia" });
            Countries.Add(new Country { CountryIndex = 58, CountryId = 58, CountryName = "Estonia" });
            Countries.Add(new Country { CountryIndex = 59, CountryId = 59, CountryName = "Israel" });
            Countries.Add(new Country { CountryIndex = 60, CountryId = 60, CountryName = "Honduras" });
            Countries.Add(new Country { CountryIndex = 61, CountryId = 61, CountryName = "Jamaica" });
            Countries.Add(new Country { CountryIndex = 62, CountryId = 62, CountryName = "Panama" });
            Countries.Add(new Country { CountryIndex = 63, CountryId = 63, CountryName = "Bolivia" });
            Countries.Add(new Country { CountryIndex = 64, CountryId = 64, CountryName = "Venezuela" });
            Countries.Add(new Country { CountryIndex = 65, CountryId = 65, CountryName = "China" });
            Countries.Add(new Country { CountryIndex = 66, CountryId = 66, CountryName = "Uzbekistan" });
            Countries.Add(new Country { CountryIndex = 67, CountryId = 67, CountryName = "Albania" });
            Countries.Add(new Country { CountryIndex = 68, CountryId = 68, CountryName = "Cyprus" });
            Countries.Add(new Country { CountryIndex = 69, CountryId = 69, CountryName = "Iceland" });
            Countries.Add(new Country { CountryIndex = 70, CountryId = 70, CountryName = "Macedonia" });
            Countries.Add(new Country { CountryIndex = 71, CountryId = 71, CountryName = "Armenia" });
            Countries.Add(new Country { CountryIndex = 72, CountryId = 72, CountryName = "Belarus" });
            Countries.Add(new Country { CountryIndex = 73, CountryId = 73, CountryName = "Georgia" });
            Countries.Add(new Country { CountryIndex = 74, CountryId = 74, CountryName = "Liechtenstein" });
            Countries.Add(new Country { CountryIndex = 75, CountryId = 75, CountryName = "Lithuania" });
            Countries.Add(new Country { CountryIndex = 76, CountryId = 76, CountryName = "Algeria" });
            Countries.Add(new Country { CountryIndex = 77, CountryId = 77, CountryName = "Benin" });
            Countries.Add(new Country { CountryIndex = 78, CountryId = 78, CountryName = "Burkina Faso" });
            Countries.Add(new Country { CountryIndex = 79, CountryId = 79, CountryName = "Cape Verde" });
            Countries.Add(new Country { CountryIndex = 80, CountryId = 80, CountryName = "Congo" });
            Countries.Add(new Country { CountryIndex = 81, CountryId = 81, CountryName = "DR Congo" });
            Countries.Add(new Country { CountryIndex = 82, CountryId = 82, CountryName = "Egypt" });
            Countries.Add(new Country { CountryIndex = 83, CountryId = 83, CountryName = "Equatorial Guinea" });
            Countries.Add(new Country { CountryIndex = 84, CountryId = 84, CountryName = "Gabon" });
            Countries.Add(new Country { CountryIndex = 85, CountryId = 85, CountryName = "Gambia" });
            Countries.Add(new Country { CountryIndex = 86, CountryId = 86, CountryName = "Guinea" });
            Countries.Add(new Country { CountryIndex = 87, CountryId = 87, CountryName = "Guinea-Bissau" });
            Countries.Add(new Country { CountryIndex = 88, CountryId = 88, CountryName = "Kenya" });
            Countries.Add(new Country { CountryIndex = 89, CountryId = 89, CountryName = "Liberia" });
            Countries.Add(new Country { CountryIndex = 90, CountryId = 90, CountryName = "Lybia" });
            Countries.Add(new Country { CountryIndex = 91, CountryId = 91, CountryName = "Mali" });
            Countries.Add(new Country { CountryIndex = 92, CountryId = 92, CountryName = "Morocco" });
            Countries.Add(new Country { CountryIndex = 93, CountryId = 93, CountryName = "Mozambique" });
            Countries.Add(new Country { CountryIndex = 94, CountryId = 94, CountryName = "Senegal" });
            Countries.Add(new Country { CountryIndex = 95, CountryId = 95, CountryName = "Sierra Leona" });
            Countries.Add(new Country { CountryIndex = 96, CountryId = 96, CountryName = "Zambia" });
            Countries.Add(new Country { CountryIndex = 97, CountryId = 97, CountryName = "Zimbabwe" });
            Countries.Add(new Country { CountryIndex = 98, CountryId = 98, CountryName = "Canada" });
            Countries.Add(new Country { CountryIndex = 99, CountryId = 99, CountryName = "Dominica" });
            Countries.Add(new Country { CountryIndex = 100, CountryId = 100, CountryName = "Guadeloupe" });
            Countries.Add(new Country { CountryIndex = 101, CountryId = 222, CountryName = "Martinica" });
            Countries.Add(new Country { CountryIndex = 102, CountryId = 223, CountryName = "Netherlands Antilles" });
            Countries.Add(new Country { CountryIndex = 103, CountryId = 224, CountryName = "Oman" });
            Countries.Add(new Country { CountryIndex = 104, CountryId = 225, CountryName = "Nation Free" });
            Countries.Add(new Country { CountryIndex = 105, CountryId = 226, CountryName = "Solid Color" });
            Countries.Add(new Country { CountryIndex = 106, CountryId = 227, CountryName = "My Team" });
            Countries.Add(new Country { CountryIndex = 107, CountryId = 228, CountryName = "Bahrein" });
            Countries.Add(new Country { CountryIndex = 108, CountryId = 229, CountryName = "Iraq" });
            Countries.Add(new Country { CountryIndex = 109, CountryId = 230, CountryName = "Jordan" });
            Countries.Add(new Country { CountryIndex = 110, CountryId = 231, CountryName = "Kuwait" });
            Countries.Add(new Country { CountryIndex = 111, CountryId = 232, CountryName = "North Korea" });
            Countries.Add(new Country { CountryIndex = 112, CountryId = 233, CountryName = "UAE" });
        }
    }
}
