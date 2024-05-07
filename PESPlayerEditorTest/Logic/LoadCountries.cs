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
            Countries.Add(new Country { CountryIndex = 0, CountryId = 249, CountryName = "Free Nationality" });
            Countries.Add(new Country { CountryIndex = 1, CountryId = 122, CountryName = "Albania" });
            Countries.Add(new Country { CountryIndex = 2, CountryId = 123, CountryName = "Austria" });
            Countries.Add(new Country { CountryIndex = 3, CountryId = 124, CountryName = "Belarus" });
            Countries.Add(new Country { CountryIndex = 4, CountryId = 125, CountryName = "Belgium" });
            Countries.Add(new Country { CountryIndex = 5, CountryId = 126, CountryName = "Bosnia" });
            Countries.Add(new Country { CountryIndex = 6, CountryId = 127, CountryName = "Bulgaria" });
            Countries.Add(new Country { CountryIndex = 7, CountryId = 128, CountryName = "Croatia" });
            Countries.Add(new Country { CountryIndex = 8, CountryId = 129, CountryName = "Cyprus" });
            Countries.Add(new Country { CountryIndex = 9, CountryId = 130, CountryName = "Czechia" });
            Countries.Add(new Country { CountryIndex = 10, CountryId = 131, CountryName = "Denmark" });
            Countries.Add(new Country { CountryIndex = 11, CountryId = 132, CountryName = "England" });
            Countries.Add(new Country { CountryIndex = 12, CountryId = 133, CountryName = "Estonia" });
            Countries.Add(new Country { CountryIndex = 13, CountryId = 134, CountryName = "Finland" });
            Countries.Add(new Country { CountryIndex = 14, CountryId = 135, CountryName = "France" });
            Countries.Add(new Country { CountryIndex = 15, CountryId = 136, CountryName = "Germany" });
            Countries.Add(new Country { CountryIndex = 16, CountryId = 137, CountryName = "Georgia" });
            Countries.Add(new Country { CountryIndex = 17, CountryId = 138, CountryName = "Greece" });
            Countries.Add(new Country { CountryIndex = 18, CountryId = 139, CountryName = "Hungary" });
            Countries.Add(new Country { CountryIndex = 19, CountryId = 140, CountryName = "Finland" });
            Countries.Add(new Country { CountryIndex = 20, CountryId = 141, CountryName = "Ireland" });
            Countries.Add(new Country { CountryIndex = 21, CountryId = 142, CountryName = "Israel" });
            Countries.Add(new Country { CountryIndex = 22, CountryId = 143, CountryName = "Italy" });
            Countries.Add(new Country { CountryIndex = 23, CountryId = 144, CountryName = "Latvia" });
            Countries.Add(new Country { CountryIndex = 24, CountryId = 145, CountryName = "Liechtenstein" });
            Countries.Add(new Country { CountryIndex = 25, CountryId = 146, CountryName = "Lithuania" });
            Countries.Add(new Country { CountryIndex = 26, CountryId = 147, CountryName = "Luxembourg" });
            Countries.Add(new Country { CountryIndex = 27, CountryId = 148, CountryName = "Macedonia" });
            Countries.Add(new Country { CountryIndex = 28, CountryId = 149, CountryName = "Moldova" });
            Countries.Add(new Country { CountryIndex = 29, CountryId = 150, CountryName = "Netherlands" });
            Countries.Add(new Country { CountryIndex = 30, CountryId = 151, CountryName = "Northern Ireland" });
            Countries.Add(new Country { CountryIndex = 31, CountryId = 152, CountryName = "Norway" });
            Countries.Add(new Country { CountryIndex = 32, CountryId = 153, CountryName = "Poland" });
            Countries.Add(new Country { CountryIndex = 33, CountryId = 154, CountryName = "Portugal" });
            Countries.Add(new Country { CountryIndex = 34, CountryId = 155, CountryName = "Romania" });
            Countries.Add(new Country { CountryIndex = 35, CountryId = 156, CountryName = "Russia" });
            Countries.Add(new Country { CountryIndex = 36, CountryId = 157, CountryName = "Scotland" });
            Countries.Add(new Country { CountryIndex = 37, CountryId = 158, CountryName = "Serbia and Montenegro" });
            Countries.Add(new Country { CountryIndex = 38, CountryId = 159, CountryName = "Slovakia" });
            Countries.Add(new Country { CountryIndex = 39, CountryId = 160, CountryName = "Slovenia" });
            Countries.Add(new Country { CountryIndex = 40, CountryId = 161, CountryName = "Spain" });
            Countries.Add(new Country { CountryIndex = 41, CountryId = 162, CountryName = "Sweden" });
            Countries.Add(new Country { CountryIndex = 42, CountryId = 163, CountryName = "Switzerland" });
            Countries.Add(new Country { CountryIndex = 43, CountryId = 164, CountryName = "Turkey" });
            Countries.Add(new Country { CountryIndex = 44, CountryId = 165, CountryName = "Ukraine" });
            Countries.Add(new Country { CountryIndex = 45, CountryId = 166, CountryName = "Uzbekistan" });
            Countries.Add(new Country { CountryIndex = 46, CountryId = 167, CountryName = "Wales" });
            Countries.Add(new Country { CountryIndex = 47, CountryId = 168, CountryName = "Algeria" });
            Countries.Add(new Country { CountryIndex = 48, CountryId = 169, CountryName = "Angola" });
            Countries.Add(new Country { CountryIndex = 49, CountryId = 170, CountryName = "Benin" });
            Countries.Add(new Country { CountryIndex = 50, CountryId = 171, CountryName = "Burkina Faso" });
            Countries.Add(new Country { CountryIndex = 51, CountryId = 172, CountryName = "Burundi" });
            Countries.Add(new Country { CountryIndex = 52, CountryId = 173, CountryName = "Cape Verde" });
            Countries.Add(new Country { CountryIndex = 53, CountryId = 174, CountryName = "Cameroon" });
            Countries.Add(new Country { CountryIndex = 54, CountryId = 175, CountryName = "Comoros" });
            Countries.Add(new Country { CountryIndex = 55, CountryId = 176, CountryName = "DR Congo" });
            Countries.Add(new Country { CountryIndex = 56, CountryId = 177, CountryName = "Cote d'Ivoire" });
            Countries.Add(new Country { CountryIndex = 57, CountryId = 178, CountryName = "Egypt" });
            Countries.Add(new Country { CountryIndex = 58, CountryId = 179, CountryName = "Ethiopia" });
            Countries.Add(new Country { CountryIndex = 59, CountryId = 180, CountryName = "Gabon" });
            Countries.Add(new Country { CountryIndex = 60, CountryId = 181, CountryName = "Gambia" });
            Countries.Add(new Country { CountryIndex = 61, CountryId = 182, CountryName = "Ghana" });
            Countries.Add(new Country { CountryIndex = 62, CountryId = 183, CountryName = "Guinea" });
            Countries.Add(new Country { CountryIndex = 63, CountryId = 184, CountryName = "Liberia" });
            Countries.Add(new Country { CountryIndex = 64, CountryId = 185, CountryName = "Libya" });
            Countries.Add(new Country { CountryIndex = 65, CountryId = 186, CountryName = "Mali" });
            Countries.Add(new Country { CountryIndex = 66, CountryId = 187, CountryName = "Mauritius" });
            Countries.Add(new Country { CountryIndex = 67, CountryId = 188, CountryName = "Morocco" });
            Countries.Add(new Country { CountryIndex = 68, CountryId = 189, CountryName = "Mozambique" });
            Countries.Add(new Country { CountryIndex = 69, CountryId = 190, CountryName = "Namibia" });
            Countries.Add(new Country { CountryIndex = 70, CountryId = 191, CountryName = "Nigeria" });
            Countries.Add(new Country { CountryIndex = 71, CountryId = 192, CountryName = "Rwanda" });
            Countries.Add(new Country { CountryIndex = 72, CountryId = 193, CountryName = "Senegal" });
            Countries.Add(new Country { CountryIndex = 73, CountryId = 194, CountryName = "Sierra Leone" });
            Countries.Add(new Country { CountryIndex = 74, CountryId = 195, CountryName = "South Africa" });
            Countries.Add(new Country { CountryIndex = 75, CountryId = 196, CountryName = "Sudan" });
            Countries.Add(new Country { CountryIndex = 76, CountryId = 197, CountryName = "Togo" });
            Countries.Add(new Country { CountryIndex = 77, CountryId = 198, CountryName = "Tunisia" });
            Countries.Add(new Country { CountryIndex = 78, CountryId = 199, CountryName = "Zambia" });
            Countries.Add(new Country { CountryIndex = 79, CountryId = 200, CountryName = "Zimbabwe" });
            Countries.Add(new Country { CountryIndex = 80, CountryId = 201, CountryName = "Bermuda" });
            Countries.Add(new Country { CountryIndex = 81, CountryId = 202, CountryName = "Canada" });
            Countries.Add(new Country { CountryIndex = 82, CountryId = 203, CountryName = "Costa Rica" });
            Countries.Add(new Country { CountryIndex = 83, CountryId = 204, CountryName = "Curacao" });
            Countries.Add(new Country { CountryIndex = 84, CountryId = 205, CountryName = "Guatemala" });
            Countries.Add(new Country { CountryIndex = 85, CountryId = 206, CountryName = "Honduras" });
            Countries.Add(new Country { CountryIndex = 86, CountryId = 207, CountryName = "Jamaica" });
            Countries.Add(new Country { CountryIndex = 87, CountryId = 208, CountryName = "Mexico" });
            Countries.Add(new Country { CountryIndex = 88, CountryId = 209, CountryName = "Panama" });
            Countries.Add(new Country { CountryIndex = 89, CountryId = 210, CountryName = "Trinidad and Tobago" });
            Countries.Add(new Country { CountryIndex = 90, CountryId = 211, CountryName = "United States" });
            Countries.Add(new Country { CountryIndex = 91, CountryId = 212, CountryName = "Argentina" });
            Countries.Add(new Country { CountryIndex = 92, CountryId = 213, CountryName = "Bolivia" });
            Countries.Add(new Country { CountryIndex = 93, CountryId = 214, CountryName = "Brazil" });
            Countries.Add(new Country { CountryIndex = 94, CountryId = 215, CountryName = "Chile" });
            Countries.Add(new Country { CountryIndex = 95, CountryId = 216, CountryName = "Colombia" });
            Countries.Add(new Country { CountryIndex = 96, CountryId = 217, CountryName = "Ecuador" });
            Countries.Add(new Country { CountryIndex = 97, CountryId = 218, CountryName = "Grenada" });
            Countries.Add(new Country { CountryIndex = 98, CountryId = 219, CountryName = "Paraguay" });
            Countries.Add(new Country { CountryIndex = 99, CountryId = 220, CountryName = "Peru" });
            Countries.Add(new Country { CountryIndex = 100, CountryId = 221, CountryName = "Uruguay" });
            Countries.Add(new Country { CountryIndex = 101, CountryId = 222, CountryName = "Suriname" });
            Countries.Add(new Country { CountryIndex = 102, CountryId = 223, CountryName = "Venezuela" });
            Countries.Add(new Country { CountryIndex = 103, CountryId = 224, CountryName = "China" });
            Countries.Add(new Country { CountryIndex = 104, CountryId = 225, CountryName = "India" });
            Countries.Add(new Country { CountryIndex = 105, CountryId = 226, CountryName = "Iran" });
            Countries.Add(new Country { CountryIndex = 106, CountryId = 227, CountryName = "Japan" });
            Countries.Add(new Country { CountryIndex = 107, CountryId = 228, CountryName = "Korea" });
            Countries.Add(new Country { CountryIndex = 108, CountryId = 229, CountryName = "Lebanon" });
            Countries.Add(new Country { CountryIndex = 109, CountryId = 230, CountryName = "Pakistan" });
            Countries.Add(new Country { CountryIndex = 110, CountryId = 231, CountryName = "Palestine" });
            Countries.Add(new Country { CountryIndex = 111, CountryId = 232, CountryName = "Qatar" });
            Countries.Add(new Country { CountryIndex = 112, CountryId = 233, CountryName = "Saudi Arabia" });
            Countries.Add(new Country { CountryIndex = 113, CountryId = 234, CountryName = "Thailand" });
            Countries.Add(new Country { CountryIndex = 114, CountryId = 235, CountryName = "Vietnam" });
            Countries.Add(new Country { CountryIndex = 115, CountryId = 236, CountryName = "Australia" });
            Countries.Add(new Country { CountryIndex = 116, CountryId = 237, CountryName = "New Zealand" });
            Countries.Add(new Country { CountryIndex = 117, CountryId = 238, CountryName = "Tahiti" });
        }
    }
}
