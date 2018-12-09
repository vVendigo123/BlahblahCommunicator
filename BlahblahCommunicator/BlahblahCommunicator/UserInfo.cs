using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlahblahCommunicator // SPRAWDŹ CZY DZIAŁA POPRAWNIE
{
    class UserInfo
    {
        public string Username { get; }
        public string Password { get; }
        public int Id { get; set; }
        public string createdAt { get; set; }
        private const int maxFriends = 10;
        public int[] friends;

        private static UserInfo user = null;

        private UserInfo(string username, string password)
        {
            Username = username;
            Password = password;
            friends = new int[maxFriends];
        }

        public static UserInfo GetInstance(string username, string password)
        {
            if (user == null)
            {
                user = new UserInfo(username, password);
            }
            return user;
        }
    }
}
