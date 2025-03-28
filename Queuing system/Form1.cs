using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Queuing_system
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Random random = new Random();
        public Stopwatch stopwatch = new Stopwatch();   //общее время выполнения
        public Stopwatch stopwatch_processing = new Stopwatch(); //будет засекаться время на обслуживание
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisX.Minimum = 0;
            chart3.ChartAreas[0].AxisX.Minimum = 0;
            //входной поток - простейший:
            //стационарен (вероятностные характеристики не зависят от времени),
            //ординарен (в элементарный промежуток времени появляется одна заявка),
            //не имеет последействий (число появления заявок в определённый промежуток времени не зависит от появления заявок в другое время)
            bool isOrdinary = radioButton2.Checked;         //ординарный ли поток (если нет, то вероятности для 1, 2, 3, 4 заявок - 0.5, 0.3, 0.15, 0.05)
            double lyambda = double.Parse(textBox2.Text);   //интенсивность потока (чаастота поялвения заявки или среднее число заявок в единицу времени)
            double mu = double.Parse(textBox3.Text); //интенсивность обслуживания заявки
            double sigma = double.Parse(textBox4.Text);   //интенсивность прихода заявки с орбиты
            int n = 0; //состояние прибора
            int N = int.Parse(textBox5.Text); //максимальное число заявок
            int i = 0;  //число заявок на орбите
            double r0 = double.Parse(textBox6.Text);    //вероятность, что обслуживание заявки будет без ошибок
            double r1 = double.Parse(textBox7.Text);    //вероятность, что заявка после обслуживания добавится в очередь
            double r2 = double.Parse(textBox8.Text);    //вероятность, что заявка после обслуживания переместится на орбиту
            double ro = lyambda / (mu * r0);            //загрузка СМО
            double a = 0;                               //среднее число заявок на орбите
            textBox1.Text += "Загрузка СМО ro = " + Math.Round(ro, 3) + Environment.NewLine; //загрузка СМО
            Dictionary<int, double> dict = new Dictionary<int, double>();

            //Если отмечено больше 2 элементов, то снимаем выделение со всех и отмечаем текущий.
            if (checkedListBox1.CheckedItems.Count > 1)
            {
                for (int j = 0; j < checkedListBox1.Items.Count; j++)
                    checkedListBox1.SetItemChecked(j, false);
                checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex, true);
            }
            double t = 0;   //текущее время
            double tMax = double.Parse(textBox9.Text);
            int events = 0;
            if (checkedListBox1.GetItemChecked(0) == true)
            {
                //основной цикл
                while (t < tMax)
                {
                    //определение времени до следующего события
                    double x = -Math.Log(random.NextDouble()) / lyambda;  //приход заявки с потока
                    double y = double.PositiveInfinity; //окончание обслуживания, если прибор свободен
                    if (n >= 1)
                    {
                        y = -Math.Log(random.NextDouble()) / mu;  //если прибор занят
                    }
                    double z = double.PositiveInfinity; //приход заявки с орбиты, если она пустая
                    if (i >= 1)
                    {
                        z = -Math.Log(random.NextDouble()) / (i * sigma);   //если на орбите i заявок
                    }
                    double delta_t = Math.Min(Math.Min(x, y), z);   //время до следующего события
                    t += delta_t;
                    events++;
                    if (dict.ContainsKey(i))
                    {
                        dict[i] += delta_t;
                    }
                    else
                    {
                        dict.Add(i, delta_t);
                    }
                    //обработка события
                    //приход заявки с потока
                    if (delta_t == x)
                    {
                        if (isOrdinary)
                        {
                            if (n == 0) //прибор свободен
                            {
                                n = 1;
                            }
                            else if (n >= N)    //мест в очереди нет, приход заявки на орбиту
                            {
                                i++;
                            }
                            else
                            {
                                n++;    //место в очереди есть
                            }
                        }
                        else
                        {
                            double r = random.NextDouble();
                            if (r <= 0.5)   //1 заявка
                            {
                                if (n == 0) //прибор свободен
                                {
                                    n = 1;
                                }
                                else if (n >= N)    //мест в очереди нет, приход заявки на орбиту
                                {
                                    i++;
                                }
                                else
                                {
                                    n++;    //место в очереди есть
                                }
                            }
                            else if ((0.5 < r) && (r <= 0.8))   //2 заявки
                            {
                                //textBox1.Text += "Пришли 2 заявки " + Environment.NewLine;
                                //textBox1.Text += "N = " + N + ", n = " + n + Environment.NewLine;
                                if (n >= N)    //мест в очереди нет, приход заявок на орбиту
                                {
                                    i += 2;
                                    //textBox1.Text += "Мест в очереди нет " + Environment.NewLine;
                                }
                                else if (N - n > 0) //места в очереди есть
                                {
                                    if (n + 2 <= N) //хватает мест для 2 заявок
                                    {
                                        n += 2;
                                        //textBox1.Text += "Места в очереди есть для 2 заявок " + Environment.NewLine;
                                    }
                                    else
                                    {
                                        //на орбиту идут оставшиеся заявки
                                        i += N - n;
                                        //textBox1.Text += "Места в очереди есть, на орбиту идут " + (N - n) + Environment.NewLine;
                                        //заполняем прибор до конца
                                        n = N;
                                        
                                    }
                                }
                                //textBox1.Text += "В итоге N = " + N + ", n = " + n + ", i = " + i + Environment.NewLine;
                            }
                            else if ((0.8 < r) && (r <= 0.95))  //3 заявки
                            {
                                if (n >= N)    //мест в очереди нет, приход заявок на орбиту
                                {
                                    i += 3;
                                }
                                else if (N - n > 0) //места в очереди есть
                                {
                                    if (n + 3 <= N) //хватает мест для 3 заявок
                                    {
                                        n += 3;
                                    }
                                    else
                                    {
                                        //на орбиту идут оставшиеся заявки
                                        i += N - n;
                                        //заполняем прибор до конца
                                        n = N;
                                    }
                                }
                            }
                            else //4 заявки
                            {
                                if (n >= N)    //мест в очереди нет, приход заявок на орбиту
                                {
                                    i += 4;
                                }
                                else if (N - n > 0) //места в очереди есть
                                {
                                    if (n + 4 <= N) //хватает мест для 4 заявок
                                    {
                                        n += 4;
                                    }
                                    else
                                    {
                                        //на орбиту идут оставшиеся заявки
                                        i += N - n;
                                        //заполняем прибор до конца
                                        n = N;
                                    }
                                }
                            }
                        }
                        
                    }
                    //окончание обслуживания
                    else if (delta_t == y)
                    {
                        double r = random.NextDouble();
                        if (r <= r0)    //заявка обработана
                        {
                            n--;
                        }
                        else if (r >= 1 - r2) //заявка переходит на орбиту
                        {
                            n--;
                            i++;
                        }
                        //если (r0 < r < r0+r1), заявка переходит из прибора в очередь (n = n-1+1 = n)
                    }
                    //приход заявки с орбиты
                    else if (delta_t == z)
                    {
                        //если есть место в очереди
                        if (n < N)
                        {
                            i--;
                            n++;
                        }
                    }
                    a += i;
                    if (tMax < 100)
                    {
                        chart1.Series[0].Points.AddXY(t, i);
                        chart2.Series[0].Points.AddXY(t, n);
                    }
                    
                }
                
            }
            else if (checkedListBox1.GetItemChecked(1) == true)
            {
                //основной цикл
                while (events < (int) tMax)
                {
                    //определение времени до следующего события
                    double x = -Math.Log(random.NextDouble()) / lyambda;  //приход заявки с потока
                    double y = double.PositiveInfinity; //окончание обслуживания, если прибор свободен
                    if (n >= 1)
                    {
                        y = -Math.Log(random.NextDouble()) / mu;  //если прибор занят
                    }
                    double z = double.PositiveInfinity; //приход заявки с орбиты, если она пустая
                    if (i >= 1)
                    {
                        z = -Math.Log(random.NextDouble()) / (i * sigma);   //если на орбите i заявок
                    }
                    double delta_t = Math.Min(Math.Min(x, y), z);   //время до следующего события
                    t += delta_t;
                    events++;
                    if (dict.ContainsKey(i))
                    {
                        dict[i] += delta_t;
                    }
                    else
                    {
                        dict.Add(i, delta_t);
                    }
                    //обработка события
                    //приход заявки с потока
                    if (delta_t == x)
                    {
                        if (isOrdinary)
                        {
                            if (n == 0) //прибор свободен
                            {
                                n = 1;
                            }
                            else if (n >= N)    //мест в очереди нет, приход заявки на орбиту
                            {
                                i++;
                            }
                            else
                            {
                                n++;    //место в очереди есть
                            }
                        }
                        else
                        {
                            double r = random.NextDouble();
                            if (r <= 0.5)   //1 заявка
                            {
                                if (n == 0) //прибор свободен
                                {
                                    n = 1;
                                }
                                else if (n >= N)    //мест в очереди нет, приход заявки на орбиту
                                {
                                    i++;
                                }
                                else
                                {
                                    n++;    //место в очереди есть
                                }
                            }
                            else if ((0.5 < r) && (r <= 0.8))   //2 заявки
                            {
                                //textBox1.Text += "Пришли 2 заявки " + Environment.NewLine;
                                //textBox1.Text += "N = " + N + ", n = " + n + Environment.NewLine;
                                if (n >= N)    //мест в очереди нет, приход заявок на орбиту
                                {
                                    i += 2;
                                    //textBox1.Text += "Мест в очереди нет " + Environment.NewLine;
                                }
                                else if (N - n > 0) //места в очереди есть
                                {
                                    if (n + 2 <= N) //хватает мест для 2 заявок
                                    {
                                        n += 2;
                                        //textBox1.Text += "Места в очереди есть для 2 заявок " + Environment.NewLine;
                                    }
                                    else
                                    {
                                        //на орбиту идут оставшиеся заявки
                                        i += N - n;
                                        //textBox1.Text += "Места в очереди есть, на орбиту идут " + (N - n) + Environment.NewLine;
                                        //заполняем прибор до конца
                                        n = N;

                                    }
                                }
                                //textBox1.Text += "В итоге N = " + N + ", n = " + n + ", i = " + i + Environment.NewLine;
                            }
                            else if ((0.8 < r) && (r <= 0.95))  //3 заявки
                            {
                                if (n >= N)    //мест в очереди нет, приход заявок на орбиту
                                {
                                    i += 3;
                                }
                                else if (N - n > 0) //места в очереди есть
                                {
                                    if (n + 3 <= N) //хватает мест для 3 заявок
                                    {
                                        n += 3;
                                    }
                                    else
                                    {
                                        //на орбиту идут оставшиеся заявки
                                        i += N - n;
                                        //заполняем прибор до конца
                                        n = N;
                                    }
                                }
                            }
                            else //4 заявки
                            {
                                if (n >= N)    //мест в очереди нет, приход заявок на орбиту
                                {
                                    i += 4;
                                }
                                else if (N - n > 0) //места в очереди есть
                                {
                                    if (n + 4 <= N) //хватает мест для 4 заявок
                                    {
                                        n += 4;
                                    }
                                    else
                                    {
                                        //на орбиту идут оставшиеся заявки
                                        i += N - n;
                                        //заполняем прибор до конца
                                        n = N;
                                    }
                                }
                            }
                        }
                    }
                    //окончание обслуживания
                    else if (delta_t == y)
                    {
                        double r = random.NextDouble();
                        if (r <= r0)    //заявка обработана
                        {
                            n--;
                        }
                        else if (r >= 1 - r2) //заявка переходит на орбиту
                        {
                            n--;
                            i++;
                        }
                        //если (r0 < r < r0+r1), заявка переходит из прибора в очередь (n = n-1+1 = n)
                    }
                    //приход заявки с орбиты
                    else if (delta_t == z)
                    {
                        //если есть место в очереди
                        if (n < N)
                        {
                            i--;
                            n++;
                        }
                    }
                    a += i;
                    if (tMax < 1000)
                    {
                        chart1.Series[0].Points.AddXY(t, i);
                        chart2.Series[0].Points.AddXY(t, n);
                    }
                }
            }
            //textBox1.Text = "t = " + t + Environment.NewLine;
            var sortedDict = new SortedDictionary<int, double>(dict);
            foreach (int key in sortedDict.Keys)
            {
                double P = sortedDict[key] / t;
                //textBox1.Text += "i = " + key + "; sum_t = " + sortedDict[key] + "; P = " + P + Environment.NewLine;
                chart3.Series[0].Points.AddXY(key, P);
            }
            a = a / events; //среднее число заявок на орбите
            textBox1.Text += "Среднее число заявок на орбите a = " + Math.Round(a, 0) + Environment.NewLine;
            //теоретическая функция 
            if (radioButton1.Checked)
            {
                
                //for (int j = 0; j < dict.Count * 10; j++)
                //{
                //    double x = j / 10;
                //    double f = 1 / (sigma * Math.Sqrt(2 * Math.PI)) * Math.Pow(Math.E, -Math.Pow((x - a), 2) / (2 * sigma * sigma));
                //    chart3.Series[1].Points.AddXY(x, f);
                //}
            }
            

        }
        //преобразование равномерной случайной величины в эксп.
        public double RandExp(double lyambda)   //lyambda - параметр распределения
        {
            double x = random.NextDouble();
            return Math.Log(1-x)/(-lyambda);
        }
        public double probability(int i)
        {
            double result = 0;
            return result;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            chart4.Series[0].Points.Clear();
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart5.Series[0].Points.Clear();
            chart5.ChartAreas[0].AxisX.Minimum = 0;
            double lyambda = double.Parse(textBox2.Text);   //интенсивность потока (чаастота поялвения заявки или среднее число заявок в единицу времени)
            double mu = double.Parse(textBox3.Text); //интенсивность обслуживания заявки
            double sigma = double.Parse(textBox4.Text);   //интенсивность прихода заявки с орбиты
            int n = 0; //состояние прибора
            int N = int.Parse(textBox5.Text); //максимальное число заявок
            int i = 0;  //число заявок на орбите
            double r0 = double.Parse(textBox6.Text);    //вероятность, что обслуживание заявки будет без ошибок
            double r1 = double.Parse(textBox7.Text);    //вероятность, что заявка после обслуживания добавится в очередь
            double r2 = double.Parse(textBox8.Text);    //вероятность, что заявка после обслуживания переместится на орбиту
            
            //зависимость i от загрузки
            for (int j = 1; j < 10; j++)
            {
                double ro = (double) j / 10;            //загрузка СМО
                lyambda = mu * r0 * ro;
                double a = 0;                               //среднее число заявок на орбите
                textBox1.Text += "Загрузка СМО ro = " + Math.Round(ro, 3) + Environment.NewLine; //загрузка СМО
                Dictionary<int, double> dict = new Dictionary<int, double>();
                double t = 0;   //текущее время
                double tMax = 100000;
                int events = 0;
                //основной цикл
                while (t < tMax)
                {
                    //определение времени до следующего события
                    double x = -Math.Log(random.NextDouble()) / lyambda;  //приход заявки с потока
                    double y = double.PositiveInfinity; //окончание обслуживания, если прибор свободен
                    if (n >= 1)
                    {
                        y = -Math.Log(random.NextDouble()) / mu;  //если прибор занят
                    }
                    double z = double.PositiveInfinity; //приход заявки с орбиты, если она пустая
                    if (i >= 1)
                    {
                        z = -Math.Log(random.NextDouble()) / (i * sigma);   //если на орбите i заявок
                    }
                    double delta_t = Math.Min(Math.Min(x, y), z);   //время до следующего события
                    t += delta_t;
                    events++;
                    if (dict.ContainsKey(i))
                    {
                        dict[i] += delta_t;
                    }
                    else
                    {
                        dict.Add(i, delta_t);
                    }
                    //обработка события
                    //приход заявки с потока
                    if (delta_t == x)
                    {
                        if (n == 0) //прибор свободен
                        {
                            n = 1;
                        }
                        else if (n >= N)    //мест в очереди нет, приход заявки на орбиту
                        {
                            i++;
                        }
                        else
                        {
                            n++;    //место в очереди есть
                        }
                    }
                    //окончание обслуживания
                    else if (delta_t == y)
                    {
                        double r = random.NextDouble();
                        if (r <= r0)    //заявка обработана
                        {
                            n--;
                        }
                        else if (r >= 1 - r2) //заявка переходит на орбиту
                        {
                            n--;
                            i++;
                        }
                        //если (r0 < r < r0+r1), заявка переходит из прибора в очередь (n = n-1+1 = n)
                    }
                    //приход заявки с орбиты
                    else if (delta_t == z)
                    {
                        //если есть место в очереди
                        if (n < N)
                        {
                            i--;
                            n++;
                        }
                    }
                    a += i;
                    
                }
                a = a / events; //среднее число заявок на орбите
                textBox1.Text += "Среднее число заявок на орбите a = " + Math.Round(a, 0) + Environment.NewLine;
                chart4.Series[0].Points.AddXY(ro, a);
            }
            lyambda = double.Parse(textBox2.Text);
            //зависимость i от N
            for (int j = 1; j < 10; j++)
            {
                N = j;
                textBox1.Text += "Число мест в очереди N = " + N + Environment.NewLine;
                Dictionary<int, double> dict = new Dictionary<int, double>();
                double a = 0;                               //среднее число заявок на орбите
                double t = 0;   //текущее время
                double tMax = 100000;
                int events = 0;
                //основной цикл
                while (t < tMax)
                {
                    //определение времени до следующего события
                    double x = -Math.Log(random.NextDouble()) / lyambda;  //приход заявки с потока
                    double y = double.PositiveInfinity; //окончание обслуживания, если прибор свободен
                    if (n >= 1)
                    {
                        y = -Math.Log(random.NextDouble()) / mu;  //если прибор занят
                    }
                    double z = double.PositiveInfinity; //приход заявки с орбиты, если она пустая
                    if (i >= 1)
                    {
                        z = -Math.Log(random.NextDouble()) / (i * sigma);   //если на орбите i заявок
                    }
                    double delta_t = Math.Min(Math.Min(x, y), z);   //время до следующего события
                    t += delta_t;
                    events++;
                    if (dict.ContainsKey(i))
                    {
                        dict[i] += delta_t;
                    }
                    else
                    {
                        dict.Add(i, delta_t);
                    }
                    //обработка события
                    //приход заявки с потока
                    if (delta_t == x)
                    {
                        if (n == 0) //прибор свободен
                        {
                            n = 1;
                        }
                        else if (n >= N)    //мест в очереди нет, приход заявки на орбиту
                        {
                            i++;
                        }
                        else
                        {
                            n++;    //место в очереди есть
                        }
                    }
                    //окончание обслуживания
                    else if (delta_t == y)
                    {
                        double r = random.NextDouble();
                        if (r <= r0)    //заявка обработана
                        {
                            n--;
                        }
                        else if (r >= 1 - r2) //заявка переходит на орбиту
                        {
                            n--;
                            i++;
                        }
                        //если (r0 < r < r0+r1), заявка переходит из прибора в очередь (n = n-1+1 = n)
                    }
                    //приход заявки с орбиты
                    else if (delta_t == z)
                    {
                        //если есть место в очереди
                        if (n < N)
                        {
                            i--;
                            n++;
                        }
                    }
                    a += i;
                }
                a = a / events; //среднее число заявок на орбите
                textBox1.Text += "Среднее число заявок на орбите a = " + a + Environment.NewLine;
                chart5.Series[0].Points.AddXY(N, a);
            }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = !radioButton2.Checked;
        }
    }
}
