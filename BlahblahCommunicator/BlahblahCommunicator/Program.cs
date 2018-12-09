using System;
using System.Timers;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data;
using MySql.Data.MySqlClient;

// VERY SIMPLE COMMUNICATOR USING MYSQL DB MADE IN C#

// Two main problems:   user gets inactive, when logging out from one of multiple apps opened simultaneously
// User does not get inactive when exiting app by clicking x button

namespace BlahblahCommunicator
{
    class Program
    {
        private static bool shouldUpdate = false;

        static void Main(string[] args)
        {
            bool done = false;

            string dbServer = "localhost";
            string dbUsername = "vvendigo";
            string dbName = "blahblah_communicator";
            string dbPort = "3306";
            string dbPassword = "password";
            bool loggedIn = false;
            dbServer = ReadString("Enter server ip: ");
            UserInfo user = null;

            string connectionString = $"server={dbServer};user={dbUsername};database={dbName};port={dbPort};password={dbPassword};";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command;
            MySqlDataReader rdr;

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                connection.Open();

                Console.WriteLine("Welcome to BlahBlahCommunicator!");




                while (!done)
                {
                    Console.WriteLine("\n\n1. Log in");
                    Console.WriteLine("2. Sign up");
                    Console.WriteLine("3. Enter the BlahBlah-Area");
                    Console.WriteLine("0. Exit");
                    int choice = ReadInt("Enter your choice: ", 0, 3);
                    Console.Clear();
                    string username, password, password2, sql;
                    switch (choice)
                    {
                        case 1:
                            do
                            {
                                if (loggedIn && user != null)
                                {
                                    Console.WriteLine("You are already logged in! Do you want to log out?");
                                    choice = ReadInt("(1)-Yes / (0)-No\n", 0, 1);
                                    switch (choice)
                                    {
                                        case 1:
                                            loggedIn = false;
                                            sql = $"DELETE FROM active_users WHERE user_id = {user.Id}";
                                            command = new MySqlCommand(sql, connection);
                                            command.ExecuteNonQuery();
                                            user = null;
                                            break;
                                        case 2:
                                            break;
                                    }
                                }
                                else
                                {
                                    username = ReadString("Enter username: ");
                                    if (username == "0")
                                        break;
                                    if (username.Length < 3)
                                    {
                                        Console.WriteLine("Username must be at least 3 characters long.");
                                        continue;
                                    }
                                    password = ReadString("Enter password: ");
                                    sql = $"SELECT id FROM users WHERE username = '{username}' AND password = '{password}'";
                                    command = new MySqlCommand(sql, connection);
                                    object result = command.ExecuteScalar();
                                    if (result != null)
                                    {
                                        sql = $"SELECT id, created_at FROM users WHERE username = '{username}' AND password = '{password}'";
                                        command = new MySqlCommand(sql, connection);
                                        rdr = command.ExecuteReader();
                                        while (rdr.Read())
                                        {
                                            user = UserInfo.GetInstance(username, password);
                                            user.Id = int.Parse(rdr[0].ToString());
                                            user.createdAt = rdr[1].ToString();
                                            loggedIn = true;

                                        }
                                        rdr.Close();

                                        sql = $"SELECT user_id FROM active_users WHERE user_id = {user.Id}";
                                        command = new MySqlCommand(sql, connection);
                                        result = null;
                                        result = command.ExecuteScalar();
                                        if (result == null)
                                        {
                                            sql = $"INSERT INTO active_users (user_id) VALUES ({user.Id})";
                                            command = new MySqlCommand(sql, connection);
                                            command.ExecuteNonQuery();
                                        }
                                        Console.WriteLine("You have logged in successfully!");
                                    }
                                    else
                                        Console.WriteLine("Wrong username or password!");
                                }
                            } while (!loggedIn || user == null);
                            break;
                        case 2:
                            bool signedIn = false;
                            do
                            {
                                username = ReadString("Enter username: ");
                                if (username == "0")
                                    break;
                                if (username.Length < 3)
                                {
                                    Console.WriteLine("Username must be at least 3 characters long.");
                                    continue;
                                }
                                sql = $"SELECT id FROM users WHERE username = '{username}'";
                                command = new MySqlCommand(sql, connection);
                                object result = command.ExecuteScalar();
                                if (result != null)
                                {
                                    Console.WriteLine("Username is already being occupied!");
                                    continue;
                                }
                                password = ReadString("Enter password: ");
                                password2 = ReadString("Confirm password: ");
                                if (password != password2)
                                    Console.WriteLine("Your passwords are not the same!");
                                else // ADDING USER TO THE DATABASE
                                {
                                    sql = $"INSERT INTO users (username, password) VALUES ('{username}', '{password}')";
                                    command = new MySqlCommand(sql, connection);
                                    command.ExecuteNonQuery();
                                    signedIn = true;
                                    Console.WriteLine("You have signed in successfully!");
                                }

                            } while (!signedIn);
                            break;
                        case 3:
                            bool leave = false;

                            while (!leave && loggedIn && user != null)
                            {

                                Console.WriteLine("1. Start chat with someone.");
                                Console.WriteLine("2. See the list of your friends.");
                                Console.WriteLine("3. See the list of active users.");
                                Console.WriteLine("0. Leave the BlahBlah-Area");

                                choice = ReadInt("Your choice: ", 0, 3);
                                Console.Clear();
                                string partnerName;
                                int partnerId = 0;
                                bool startChat = false;
                                switch (choice)
                                {
                                    case 1:
                                        while (!startChat)
                                        {
                                            partnerName = ReadString("Enter the username of a person you want to chat with: ");
                                            if (partnerName == "0")
                                                break;
                                            sql = $"SELECT id FROM users WHERE username = '{partnerName}'";
                                            command = new MySqlCommand(sql, connection);
                                            object result = command.ExecuteScalar();
                                            if (result != null)
                                            {
                                                partnerId = int.Parse(result.ToString());
                                                if (user.Id == partnerId)
                                                {
                                                    Console.WriteLine("You cannot chat with yourself!");
                                                    startChat = false;
                                                }
                                                else
                                                    startChat = true;
                                            }
                                            else
                                                Console.WriteLine("No users with given name!");
                                        }
                                        if (startChat && partnerId != 0)
                                        {
                                            string message = "";
                                            int numberOfMessages = 20;
                                            string[] conversation = new string[numberOfMessages];
                                            bool refresh = false;
                                            ConsoleKeyInfo pressedKey;
                                            bool endChat = false;
                                            startChat = false;

                                            SetTimer();

                                            while (!endChat)
                                            {
                                                if (Console.KeyAvailable)
                                                {
                                                    pressedKey = Console.ReadKey();
                                                    if (pressedKey.Key == ConsoleKey.Escape)
                                                    {
                                                        endChat = true;
                                                        continue;
                                                    }
                                                    else if (pressedKey.Key == ConsoleKey.Backspace)
                                                    {
                                                        if (message.Length > 0)
                                                        {
                                                            message = "" + message.Substring(0, message.Length - 1);
                                                        }
                                                    }
                                                    else if (pressedKey.Key == ConsoleKey.Enter)
                                                    {
                                                        sql = "INSERT INTO messages(writer_id, receiver_id, content)" +
                                                              $"VALUES({user.Id}, {partnerId}, '{message}')";
                                                        command = new MySqlCommand(sql, connection);
                                                        command.ExecuteNonQuery();
                                                        message = "";
                                                        shouldUpdate = true;
                                                    }
                                                    else
                                                        message += pressedKey.KeyChar;
                                                    refresh = true;

                                                }
                                                if (shouldUpdate)
                                                {
                                                    sql = $"SELECT username, content FROM messages INNER JOIN users ON users.id = writer_id" +
                                                            $" WHERE (writer_id = {user.Id} AND receiver_id = {partnerId}) OR (writer_id = {partnerId} AND receiver_id = {user.Id})" +
                                                            "ORDER BY messages.created_at DESC";
                                                    command = new MySqlCommand(sql, connection);
                                                    rdr = command.ExecuteReader();
                                                    for (int i = numberOfMessages - 1; i >= 0 && rdr.Read(); i--)
                                                    {
                                                        if (conversation[i] != rdr[0].ToString() + ": " + rdr[1])
                                                            refresh = true;
                                                        conversation[i] = rdr[0].ToString() + ": " + rdr[1];
                                                    }
                                                    rdr.Close();
                                                    shouldUpdate = false;
                                                }
                                                if (refresh)
                                                {
                                                    Console.Clear();
                                                    Console.WriteLine("Click 'ESC' to exit the chat.\n");
                                                    for (int i = 0; i < numberOfMessages; i++)
                                                    {
                                                        Console.WriteLine(conversation[i]);
                                                    }
                                                    Console.Write(message);
                                                    refresh = false;
                                                }

                                            }
                                            aTimer.Stop();
                                            aTimer.Dispose();
                                            Console.Clear();
                                        }
                                        break;
                                    case 2:
                                        sql = $"SELECT username FROM users INNER JOIN friends ON id = friend_id WHERE user_id = {user.Id}";
                                        command = new MySqlCommand(sql, connection);
                                        rdr = command.ExecuteReader();
                                        Console.WriteLine("Friends:\n\n");
                                        while (rdr.Read())
                                        {
                                            Console.WriteLine(rdr[0]);
                                        }
                                        rdr.Close();
                                        Console.ReadKey();
                                        Console.Clear();
                                        break;
                                    case 3:
                                        sql = "SELECT username FROM users INNER JOIN active_users ON id = user_id";
                                        command = new MySqlCommand(sql, connection);
                                        rdr = command.ExecuteReader();
                                        Console.WriteLine("Active users:\n\n");
                                        while (rdr.Read())
                                        {
                                            Console.WriteLine(rdr[0]);
                                        }
                                        rdr.Close();
                                        Console.ReadKey();
                                        Console.Clear();
                                        break;
                                    case 0:
                                        leave = true;
                                        break;
                                }
                            }
                            break;
                        case 0:
                            if (loggedIn == true || user != null)
                            {
                                if (loggedIn == true)
                                    loggedIn = false;
                                if (user != null)
                                {
                                    sql = $"DELETE FROM active_users WHERE user_id = {user.Id}";
                                    command = new MySqlCommand(sql, connection);
                                    command.ExecuteNonQuery();
                                    user = null;
                                }
                            }
                            done = true;
                            break;
                    }
                    //Program
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            connection.Close();
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        // TIMER

        private static System.Timers.Timer aTimer;
        private static void SetTimer()
        {
            // Create a timer with a one second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            shouldUpdate = true;
        }
        //static UserInfo SignUp()
        //{

        //}

        // READING INPUT

        // Reading STRING
        static string ReadString(string prompt = "Enter text: ", int min = 1, string error = "You must enter at least one character!")
        {
            bool isGood = false;
            string outcome;
            do
            {
                Console.Write(prompt);
                outcome = Console.ReadLine();
                if (outcome.Length >= min)
                    isGood = true;
                if (!isGood)
                    Console.WriteLine(error);
            } while (!isGood);
            return outcome;
        }
        // Reading INT
        static int ReadInt(string prompt = "Enter a number: ", int min = int.MinValue, int max = int.MaxValue, string error = "You have entered a wrong number!")
        {
            bool isGood = false;
            int outcome;
            do
            {
                bool isDouble = int.TryParse(ReadString(prompt), out outcome);
                if ((outcome >= min) && (outcome <= max) && isDouble)
                    isGood = true;
                if (!isGood)
                    Console.WriteLine(error);
            } while (!isGood);
            return outcome;
        }
    }
}
