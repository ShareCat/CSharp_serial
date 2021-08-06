using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;
namespace SerialCommunicate
{
    public partial class Form1 : Form
    {
        int receive_len;  //接收累计
        int sent_len;     //发送累计
        string newline_str = "\r\n";  //换行字符串
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            serialPort1.Encoding = Encoding.GetEncoding("GB2312"); 
        }

        private void button1_Click(object sender, EventArgs e)  //搜索按钮按下
        {
            SearchAndAddSerialToComboBox(serialPort1, comboBox1);  //搜索可用串口
        }

        private void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
        {                                                               //将可用端口号添加到ComboBox
            //string[] MyString = new string[20];                         //最多容纳20个，太多会影响调试效率
            string Buffer;                                              //缓存
            MyBox.Items.Clear();                                        //清空ComboBox内容
            //int count = 0;
            for (int i = 1; i < 50; i++)                                //循环
            {
                try                                                     //核心原理是依靠try和catch完成遍历
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();                                      //如果失败，后面的代码不会执行
                    // MyString[count] = Buffer;
                    MyBox.Items.Add(Buffer);                            //打开成功，添加至下俩列表
                    MyPort.Close();                                     //关闭
                    //count++;
                }
                catch
                {
                    //count--;
                }
            }
            //MyBox.Text = MyString[0];                                   //初始化
        }
             

        private void Form1_Load(object sender, EventArgs e)
        {
            SearchAndAddSerialToComboBox(serialPort1 , comboBox1);  //搜索可用串口
            
            comboBox2.Text = "115200";  //波特率默认值
            comboBox3.Text = "8";       //数据位默认值
            comboBox4.Text = "One";     //停止位默认值
            comboBox5.Text = "None";    //奇偶校验默认值
            comboBox6.Text = "String";  //接收编码默认值
            comboBox7.Text = "String";  //发送编码默认值
            comboBox8.Text = "AP";      //WIFI模式默认值
            comboBox9.Text = "OPEN";    //AP模式加密方式默认值 
            comboBox10.Text = "GPIO0"; //GPIO默认值 

            /*****************非常重要************************/
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//必须手动添加事件处理程序

        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)//串口数据接收事件
        {
            
            if (comboBox6.Text == "String")//如果接收模式为字符模式
            {
                string str = serialPort1.ReadExisting();//字符串方式读
                textBox1.AppendText(str);//添加内容
                receive_len = Convert.ToInt32(label11.Text) + str.Length;  //累计收到的字符个数
                label11.Text = Convert.ToString(receive_len);  //显示累计收到的字符个数
                if (receive_len > 30000) receive_len = 0;   //清零
            }
            else //如果接收模式为数值接收
            {           
                byte[] data = new byte[serialPort1.BytesToRead];          //定义缓冲区，因为串口事件触发时有可能收到不止一个字节
                serialPort1.Read(data, 0, data.Length);
                foreach (byte Member in data)                             //遍历用法
                {
                    string str = Convert.ToString(Member, 16).ToUpper();
                    textBox1.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " ");
                }
                receive_len = Convert.ToInt32(label11.Text) + data.Length;  //累计收到的字符个数
                label11.Text = Convert.ToString(receive_len);  //显示累计收到的字符个数
                if (receive_len > 30000) receive_len = 0;   //清零
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Open")
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text, 10);//十进制数据转换
                    serialPort1.DataBits = Convert.ToInt32(comboBox3.Text, 10);//十进制数据转换
                    serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBox4.Text);
                    serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), comboBox5.Text);
                    serialPort1.Open();
                    button2.Text = "Close";
                    button1.Enabled = false;//打开串口按钮不可用
                    comboBox1.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;                 
                }
                catch
                {
                    MessageBox.Show("Serial port open failed", "Error!!!");
                }
            }
            else
            {
                try
                {
                    serialPort1.Close();//关闭串口
                    button2.Text = "Open";
                    button1.Enabled = true;//打开串口按钮可用
                    comboBox1.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("Serial port close failed", "Error!!!");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] Data = new byte[1];
            string str;
            if (serialPort1.IsOpen)//判断串口是否打开，如果打开执行下一步操作
            {
                if (textBox2.Text != "")
                {
                    if (comboBox7.Text == "String")//如果发送模式是字符模式
                    {
                        try
                        {
                            serialPort1.Write(textBox2.Text);//写数据
                            textBox3.AppendText(textBox2.Text);
                            sent_len = Convert.ToInt32(label12.Text) + textBox2.Text.Length;  //累计发送的字符个数
                            label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                            if (sent_len > 30000) sent_len = 0;   //清零
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("Serial data transmission error !", "Error!!!"); ;//出错提示
                            serialPort1.Close();
                            button1.Enabled = true;//打开串口按钮可用
                        }
                    }
                    else
                    {                  
                        try          //如果此时用户输入字符串中含有非法字符（字母，汉字，符号等等，try，catch块可以捕捉并提示）
                        {
                            sent_len = Convert.ToInt32(label12.Text);  //获取当前已经发送的字符个数
                            for (int i = 0; i < (textBox2.Text.Length - textBox2.Text.Length % 2) / 2; i++)//转换偶数个
                            {
                                Data[0] = Convert.ToByte(textBox2.Text.Substring(i * 2, 2), 16);           //转换
                                serialPort1.Write(Data, 0, 1);                               //串口十六进制发送
                                str = Convert.ToString(Data[0], 16).ToUpper();               //转化成大写
                                textBox3.AppendText("0x" + str + " ");  //将成功发送的十六进制数在发送区显示s
                                sent_len++;   //发送累加
                            }
                            if (textBox2.Text.Length % 2 != 0)
                            {
                                Data[0] = Convert.ToByte(textBox2.Text.Substring(textBox2.Text.Length - 1, 1), 16);//单独处理最后一个字符
                                serialPort1.Write(Data, 0, 1);                              //串口十六进制发送
                                str = Convert.ToString(Data[0], 16).ToUpper();              //转化成大写
                                textBox3.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " ");  //将成功发送的十六进制数在发送区显示
                                sent_len++;   //发送累加
                            }
                            label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                            if (sent_len > 30000) sent_len = 0;   //清零
                            //Data = Convert.ToByte(textBox2.Text.Substring(textBox2.Text.Length - 1, 1), 16);
                            //  }
                        }
                        catch
                        {
                            MessageBox.Show("Please enter hexadecimal number !", "Send Error");  //提示输入十六进制数才能发送
                        }
                   }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)  //SUNFOUNDER_VER版本查询指令
        {
            string str = "AT+SUNFOUNDER_VER?";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button5_Click(object sender, EventArgs e)  //复位指令
        {
            string str = "AT+RST";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button6_Click(object sender, EventArgs e)  //AT测试
        {
            string str = "AT";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button7_Click(object sender, EventArgs e)  //WIFI模式查询
        {
            string str = "AT+CWMODE?";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)  //波特率选择框的值变化事件
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text, 10);//十进制数据转换
                }
                catch
                {
                    MessageBox.Show("Baud Rate change falied", "Error");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)  //清空接收区
        {
            textBox1.Text = "";
            label11.Text = "0";
        }

        private void button9_Click(object sender, EventArgs e)  //清空发送区
        {
            textBox3.Text = "";
            label12.Text = "0";
        }

        private void button10_Click(object sender, EventArgs e)  //扫描WIFI
        {
            string str = "AT+CWLAP";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button11_Click(object sender, EventArgs e)  //断开WIFI
        {
            string str = "AT+CWQAP";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button12_Click(object sender, EventArgs e)  //查询STA模式IP
        {
            string str = "AT+CIFSR";
            if (serialPort1.IsOpen)
            {           
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button13_Click(object sender, EventArgs e)  //接入设备的IP查询
        {
            string str = "AT+CWLIF";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button14_Click(object sender, EventArgs e)  //WIFI模式设置
        {
            string str = "AT+CWMODE=";
            string str1;
            if (comboBox8.Text == "AP") str1 = "2";
            else if (comboBox8.Text == "STA") str1 = "1";
            else str1 = "3";
            if (serialPort1.IsOpen)
            {
                    try
                    {
                        serialPort1.Write(str + str1 + newline_str);
                        textBox3.AppendText(str + str1 + newline_str);
                        sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length + newline_str.Length;  //累计发送的字符个数
                        label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                        if (sent_len > 30000) sent_len = 0;   //清零
                    }
                    catch
                    {
                        MessageBox.Show("Serial data transmission error !", "Error!!!");
                        serialPort1.Close();
                        button1.Enabled = true;
                    }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button15_Click(object sender, EventArgs e)  //加入WIFI网络：AT+CWJAP="juhao","sunfounder"
        {
            string str = "AT+CWJAP=";
            string str1 = "\"";
            string str2 = ",";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + str1 + textBox4.Text + str1 + str2 + str1 + textBox5.Text + str1 + newline_str);
                    textBox3.AppendText(str + str1 + textBox4.Text + str1 + str2 + str1 + textBox5.Text + str1 + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length * 4 + textBox4.Text.Length +
                        str2.Length + textBox5.Text.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button18_Click(object sender, EventArgs e)  //设置AP模式参数：AT+CWSAP="SUNFOUNDER","123456789",11,3
        {
            string str = "AT+CWSAP=";
            string str1 = "\"";
            string str2 = ",";
            string str3 = ",11,";
            string str4;
            if (comboBox9.Text == "OPEN") str4 = "0";
            else if (comboBox9.Text == "WPA_PSK") str4 = "2";
            else if (comboBox9.Text == "WPA2_PSK") str4 = "3";
            else  str4 = "4";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + str1 + textBox6.Text + str1 + str2 + str1 + textBox7.Text + str1 + str3 + str4 + newline_str);
                    textBox3.AppendText(str + str1 + textBox6.Text + str1 + str2 + str1 + textBox7.Text + str1 + str3 + str4 + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length *4 + textBox6.Text.Length +
                        str2.Length + textBox7.Text.Length + str3.Length + str4.Length + newline_str.Length ;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button17_Click(object sender, EventArgs e)  //设置STA模式IP地址：AT+CIPSTA="192.168.1.200"
        {
            string str = "AT+CIPSTA=";
            string str1 = "\"";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + str1 + textBox8.Text + str1 + newline_str);
                    textBox3.AppendText(str + str1 + textBox8.Text + str1 + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length + textBox8.Text.Length +
                        str1.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button19_Click(object sender, EventArgs e)  //设置AP模式IP地址：AT+CIPAP="192.168.4.1"
        {
            string str = "AT+GPIO_HIGH=";
            string str1 = "\"";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + str1 + textBox9.Text + str1 + newline_str);
                    textBox3.AppendText(str + str1 + textBox9.Text + str1 + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length + textBox9.Text.Length +
                        str1.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                    
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string str = "AT+GPIO_HIGH=";
            string str1 = "";
            if (comboBox10.Text == "GPIO0") str1 = "0";
            if (comboBox10.Text == "GPIO2") str1 = "2";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + str1 + newline_str);
                    textBox3.AppendText(str + str1 + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            string str = "AT+GPIO_LOW=";
            string str1 = "";
            if (comboBox10.Text == "GPIO0") str1 = "0";
            if (comboBox10.Text == "GPIO2") str1 = "2";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + str1 + newline_str);
                    textBox3.AppendText(str + str1 + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            string str = "AT+GPIO_READ=";
            string str1 = "";
            if (comboBox10.Text == "GPIO0") str1 = "0";
            if (comboBox10.Text == "GPIO2") str1 = "2";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + str1 + newline_str);
                    textBox3.AppendText(str + str1 + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + str1.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

        private void button22_Click(object sender, EventArgs e)  //查询芯片ID
        {
            string str = "AT+CHIP_ID?";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error");
            }
        }

        private void button23_Click(object sender, EventArgs e)  //查询连接到WIFI的状态
        {
            string str = "AT+CIPSTATUS";
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(str + newline_str);
                    textBox3.AppendText(str + newline_str);
                    sent_len = Convert.ToInt32(label12.Text) + str.Length + newline_str.Length ;  //累计发送的字符个数
                    label12.Text = Convert.ToString(sent_len);  //显示累计发送的字符个数
                    if (sent_len > 30000) sent_len = 0;   //清零
                }
                catch
                {
                    MessageBox.Show("Serial data transmission error !", "Error!!!");
                    serialPort1.Close();
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Serial port is closed !", "Error!!!");
            }
        }

    }
}
