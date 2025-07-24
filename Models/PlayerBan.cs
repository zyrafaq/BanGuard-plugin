namespace BanGuard.Models;

public class PlayerBan
{
    public bool IsBanned { get; set; }
    public bool IsProxy { get; set; }
    public List<Dictionary<dynamic, dynamic>> Bans { get; set; }

    public PlayerBan(bool isBanned, bool isProxy, List<Dictionary<dynamic, dynamic>> bans)
    {
        IsBanned = isBanned;
        IsProxy = isProxy;
        Bans = bans;

    }
}


// {
//   "banned": true,
//   "is_proxy": true,
//   "bans": [
//     {
//       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//       "server": {
//         "id": 0,
//         "name": "string"
//       },
//       "category": "string",
//       "creation_timestamp": "2025-07-24T17:58:53.327Z"
//     },
//     {
//       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//       "server": {
//         "id": 0,
//         "name": "string"
//       },
//       "category": "string",
//       "creation_timestamp": "2025-07-24T17:58:53.327Z"
//     }
//   ],
//   "ban categories": [
//     "string"
//   ],
//   "logged in as": "string"
// }