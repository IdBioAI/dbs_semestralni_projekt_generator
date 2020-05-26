using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Generator
{
    class Machine
    {
        /** POMOCNÉ ÚDAJE PRO GENERÁTOR **/
        byte numTenzo;
        int RandomTimeSend; // v sec
        /** ÚDAJE STROJE **/
        String VIN;
        String SN;
        String GPS;
        byte state;
        byte StateBatt;
        decimal X, Y, Z;
        String time;
        Tenzo[] tenzo;
        String value;

        String debug;

        private static Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        MySqlConnection connection;
        MySqlCommand mySqlCommand;

        public Machine()
        {
            Init();
            connection = Program.GetConnection();
            connection.Open();
        }

        private void Init()
        {
            VIN = RandomString(20);
            SN = RandomString(5);

            numTenzo = System.Convert.ToByte(RandomInt(1, 5));
            RandomTimeSend = System.Convert.ToByte(RandomInt(1, 10));

            tenzo = new Tenzo[numTenzo];
            for (int i = 0; i < numTenzo; i++)
            {
                tenzo[i] = new Tenzo();
            }

        }

        private void Generate()
        {
            GPS = RandomDouble(1, 50) + ";" + RandomDouble(2, 20);
            state = System.Convert.ToByte(RandomInt(0, 5));
            StateBatt = System.Convert.ToByte(RandomInt(0, 100));
            X = System.Convert.ToDecimal(RandomDouble(1, 5));
            Y = System.Convert.ToDecimal(RandomDouble(1, 5));
            Z = System.Convert.ToDecimal(RandomDouble(1, 5));

            time = DateTime.Now.AddDays(RandomInt(0, 30)).AddMinutes(RandomInt(0, 60)).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public double RandomDouble(double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public int RandomInt(int from, int to)
        {
            return random.Next(from, to + 1);
        }

        public string RandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void SendData(Object source, ElapsedEventArgs e)
        {
            if (Program.GetStop())
            {
                return;
            }

            Generate();

            debug += "Poslano.... VIN: " + VIN + "(" + RandomTimeSend + ") ";
            debug += "GPS: " + GPS + " state: " + state + " batt: " + StateBatt + " numTenzo: " + numTenzo;

            try
            {
                
                mySqlCommand = new MySqlCommand("INSERT INTO " + Program.GetSchema() + ".udajestroj " +
                                                            "(IoTPrvek_SerioveCislo, stroj_VIN, GPS, stav, stavBaterie, X, Y, Z, Datum)" +
                                                            " VALUES (N'" + SN + 
                                                            "', N'" + VIN + 
                                                            "', N'" + GPS +
                                                            "'," + state +
                                                            ", " + StateBatt +
                                                            ", N'" + System.Convert.ToString(X).Replace(',','.') +
                                                            "', N'" + System.Convert.ToString(Y).Replace(',', '.') +
                                                            "', N'" + System.Convert.ToString(Z).Replace(',', '.') + 
                                                            "', '" + time + 
                                                            "');", connection);

                mySqlCommand.ExecuteNonQuery();

                // POSÍLÁNÍ tanzometrů

                for (int i = 0; i < numTenzo; i++)
                {
                    value = System.Convert.ToString(tenzo[i].GetValue()).Replace(',', '.');
                    mySqlCommand = new MySqlCommand("INSERT INTO " + Program.GetSchema() + ".udajeTenzometr " +
                                                                "(udajeStroj_stroj_VIN, udajeStroj_Datum, oznaceniTenzo, hodnota)" +
                                                                " VALUES (N'" + VIN +
                                                                "', N'" + time +
                                                                "', N'" + tenzo[i].GetSn() +
                                                                "', '" + value +
                                                                "');", connection);

                    mySqlCommand.ExecuteNonQuery();

                    debug += "  tenzo: " + tenzo[i].GetSn() + " value: " + value;

                }

               // connection.Close();
                Program.AddSum();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Chyba při ukládání: " + ex.Message);
            }

            //Console.WriteLine(debug);
            //Console.WriteLine();
            //Console.WriteLine();
            debug = "";
        }

        public String GetVIN()
        {
            return VIN;
        }

        public String GetSN()
        {
            return SN;
        }

        public String GetTime()
        {
            return time;
        }

        public int GetRandomTimeSend()
        {
            return RandomTimeSend;
        }

    }
}
