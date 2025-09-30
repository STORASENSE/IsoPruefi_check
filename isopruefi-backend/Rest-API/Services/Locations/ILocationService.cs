using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rest_API.Services.Locations;

public interface ILocationService
{
    Task<IEnumerable<string>> GetCitiesByPlzAsync(string plz, bool onlyShortNames = true);
}