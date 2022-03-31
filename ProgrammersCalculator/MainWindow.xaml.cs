using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Buffers.Binary;

namespace ProgrammersCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /* Testing My Git Repo */
        Func<string, string> ParserPtr = DecimalToBinary;

        private static bool bigEndian        = true;
        private static bool hexToFloat       = true;
        private static bool singlePrecision  = true;

        private static LogicalType logicalType = LogicalType.AND;
        private static OutputType outputType = OutputType.Binary;

        enum InputType : int
        { 
            Decimal,
            Binary,
            Hex
        }

        enum OutputType : int
        { 
            Decimal,
            Binary,
            Hex
        }

        enum LogicalType : int
        { 
            AND,
            OR,
            XOR,
            NOT,
            LEFT,
            RIGHT
        }

        public MainWindow()
        {
            InitializeComponent();
            rbOutputBinary.IsChecked       = true;
            rbLogicalAnd.IsChecked         = true;
            rbBigEndian.IsChecked          = true;
            rbSinglePrecision.IsChecked    = true;
        }

        private static string DecimalToBinary(string s)
        {   
            return Convert.ToString(Convert.ToInt32(s, 10), 2);
        }

        private static string DecimalToHex(string s)
        {
            return Convert.ToString(Convert.ToInt32(s, 10), 16);
        }

        private static string BinaryToDecimal(string s)
        {
            //s = s.Trim();
            //if (String.IsNullOrEmpty(s)) { return ""; }
            //if (!Regex.IsMatch(s, "^[01]+$")) { return "Nan"; };
            return Convert.ToString(Convert.ToInt32(s.Replace("0b",""), 2), 10);
        }

        private static string BinaryToHex(string s)
        {
            return Convert.ToString(Convert.ToInt32(s.Replace("0b", "0"), 2), 16);
        }

        private static string HexToDecimal(string s)
        {
            return Convert.ToString(Convert.ToInt32(s.Replace("0x","0"), 16), 10);
        }

        private static string HexToBinary(string s)
        {
            return Convert.ToString(Convert.ToInt32(s.Replace("0x","0"), 16), 2);
        }

        static InputType InputCheck(string s)
        {
            if (s.StartsWith("0x"))
                return InputType.Hex;

            if (s.StartsWith("0b"))
                return InputType.Binary;
  
            return InputType.Decimal;
        }

        public static string RemoveWhitespace(string s)
        {
            return string.Join("", s.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        private string CapitalizeHex(string s)
        {
            return s.ToUpper().Replace("X", "x");
        }

        private void ConvertInput()
        {
            var tb = tbConversionInput;

            var s = tb.Text;

            if (String.IsNullOrEmpty(s)) { return; }

            /* Disable callback (prevents recursive loop)*/
            tb.TextChanged -= ConversionInputChanged;
            s = RemoveWhitespace(s);

            /* Try block fixes input cases I don't want to deal with */
            try
            {
                switch (InputCheck(s))
                {
                    case InputType.Decimal:
                        if (outputType == OutputType.Binary)
                        {
                            OutputTextBox.Text = "0b" + DecimalToBinary(s);
                        }
                        else if (outputType == OutputType.Hex)
                        {
                            OutputTextBox.Text = "0x" + CapitalizeHex(DecimalToHex(s));
                        }
                        else
                        {
                            OutputTextBox.Text = s;
                        }
                        break;
                    case InputType.Binary:
                        if (outputType == OutputType.Decimal)
                        {
                            OutputTextBox.Text = BinaryToDecimal(s);
                        }

                        else if (outputType == OutputType.Hex)
                        {
                            OutputTextBox.Text = "0x" + CapitalizeHex(BinaryToHex(s));
                        }
                        else
                        {
                            OutputTextBox.Text = s;
                        }
                        break;
                    case InputType.Hex:
                        var pos = tb.CaretIndex;
                        tb.Text = CapitalizeHex(s);
                        tb.CaretIndex = pos;
                        if (outputType == OutputType.Decimal)
                        {
                            OutputTextBox.Text = HexToDecimal(s);
                        }
                        else if (outputType == OutputType.Binary)
                        {
                            OutputTextBox.Text = "0b" + HexToBinary(s);
                        }
                        else
                        {
                            OutputTextBox.Text = tb.Text;
                        }
                        break;
                }
            }
            catch { }

            /* Re-Enable callback */
            tb.TextChanged += ConversionInputChanged;
        }

        private void ConversionInputChanged(object sender, TextChangedEventArgs e)
        {
            ConvertInput();
        }

        private static (UInt32, InputType) ParseLogicalInput(string s)
        {
            if (String.IsNullOrEmpty(s)) { return (0, InputType.Binary); }
            
            UInt32 n;

            var type = InputCheck(s);

            if (type == InputType.Hex)
                n = Convert.ToUInt32(s.Replace("0x", "0"), 16);
            else if (type == InputType.Binary)
                n = Convert.ToUInt32(s.Replace("0b", "0"), 2);
            else
                n = Convert.ToUInt32(s, 10);

            return (n, type);
        }

        private void FormatInputForHex (TextBox tb)
        {
            tb.TextChanged -= LogicalTextChanged;

            var pos = tb.CaretIndex;
            tb.Text = CapitalizeHex(tb.Text);
            tb.CaretIndex = pos;

            tb.TextChanged += LogicalTextChanged;
        }

        private void FormatInputForHex2(TextBox tb)
        {
            tb.TextChanged -= FloatingPointCallback;

            var pos = tb.CaretIndex;
            tb.Text = CapitalizeHex(tb.Text);
            tb.CaretIndex = pos;

            tb.TextChanged += FloatingPointCallback;
        }

        private void ManualLogicInputChanged()
        {
            var tb1 = tbLogicalInput1;
            var tb2 = tbLogicalInput2;

            UInt32 n1, n2, ans;
            InputType it1, it2;
            try
            {
                (n1, it1) = ParseLogicalInput(RemoveWhitespace(tb1.Text));
                (n2, it2) = ParseLogicalInput(RemoveWhitespace(tb2.Text));
            }
            catch { return; }


            if (it1 == InputType.Hex)
                FormatInputForHex(tb1);

            if (it2 == InputType.Hex)
                FormatInputForHex(tb2);

            if (String.IsNullOrEmpty(tb1.Text) || String.IsNullOrEmpty(tb2.Text)) { return; }

            switch (logicalType)
            {
                case LogicalType.AND:
                    ans = n1 & n2;
                    break;
                case LogicalType.OR:
                    ans = n1 | n2;
                    break;
                case LogicalType.XOR:
                    ans = n1 ^ n2;
                    break;
                case LogicalType.NOT:
                    ans = ~n1;
                    break;
                case LogicalType.LEFT:
                    ans = n1 << (Int32)n2;
                    break;
                case LogicalType.RIGHT:
                    ans = n1 >> (Int32)n2;
                    break;
                default:
                    ans = 0;
                    break;
            }

            tbLogicalsBinOutput.Text = "0b" + Convert.ToString(ans, 2);
            tbLogicalsDecOutput.Text = Convert.ToString(ans, 10);
            tbLogicalsHexOutput.Text = "0x" + CapitalizeHex(Convert.ToString(ans, 16));
        }

        private void LogicalTextChanged(object sender, TextChangedEventArgs e)
        {
            ManualLogicInputChanged();
        }


        private void EndiannessCallback(object sender, RoutedEventArgs e)
        {
            bigEndian = (rbBigEndian.IsChecked ?? false) ? true : false;
            ConvertFloat();
        }

        private void ConvertFloat()
        {

            var tb1 = tbFloatingPointInput;
            var tb2 = tbFloatingPointOutput;

            RemoveWhitespace(tb1.Text);

            String s = tb1.Text;
            float f;
            double d;
            byte[] tmp;

            if (String.IsNullOrEmpty(s)) { return; }

            InputType type = InputCheck(s);

            try {
                if (type == InputType.Hex)
                {
                    FormatInputForHex2(tb1);
                    if (singlePrecision)
                    {
                        tmp = BitConverter.GetBytes(Convert.ToUInt32(s.Replace("0x", "0"), 16));
                        if (!bigEndian)
                        {
                            f = BitConverter.ToSingle(tmp.Reverse().ToArray(), 0);
                        }
                        else
                        {
                            f = BitConverter.ToSingle(tmp, 0);
                        }

                        tb2.Text = String.Format("{0:E}", f);
                    }
                    else
                    {
                        tmp = BitConverter.GetBytes(Convert.ToUInt64(s.Replace("0x", "0"), 16));
                        if (!bigEndian)
                        {
                            d = BitConverter.ToDouble(tmp.Reverse().ToArray(), 0);
                        }
                        else
                        {
                            d = BitConverter.ToDouble(tmp, 0);
                        }

                        tb2.Text = String.Format("{0:E}", d);
                    }

                }

                if (type == InputType.Decimal)
                {
                    if (singlePrecision)
                    {
                        string s2;
                        f = Convert.ToSingle(s);
                        if (!bigEndian)
                        {
                            s2 = "0x" + BitConverter.ToInt32(BitConverter.GetBytes(f).Reverse().ToArray(), 0).ToString("X8");
                        }
                        else
                        {
                            s2 = "0x" + BitConverter.ToInt32(BitConverter.GetBytes(f)).ToString("X8");
                        }
                        tb2.Text = CapitalizeHex(s2);
                    }
                    else
                    {
                        string s2;
                        d = Convert.ToDouble(s);
                        if (!bigEndian)
                        {
                            s2 = "0x" + BitConverter.ToInt64(BitConverter.GetBytes(d).Reverse().ToArray(), 0).ToString("X16");
                        }
                        else
                        {
                            s2 = "0x" + BitConverter.ToInt64(BitConverter.GetBytes(d)).ToString("X16");
                        }
                        tb2.Text = CapitalizeHex(s2);
                    }

                }
            } catch { }
        }

        private void FloatingPointCallback(object sender, TextChangedEventArgs e)
        {
            ConvertFloat();
        }

        private void FloatingPointDirectionCallback(object sender, RoutedEventArgs e)
        {
            singlePrecision = (rbSinglePrecision.IsChecked ?? false) ? true : false;
            ConvertFloat();
        }

        private void LogicalsTypeChanged(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            tblLogicalOperator.Text = rb.Content.ToString();
            
            switch (rb.Content.ToString())
            {
                case "AND":
                    logicalType = LogicalType.AND;
                    break;
                case "OR":
                    logicalType = LogicalType.OR;
                    break;
                case "XOR":
                    logicalType= LogicalType.XOR;
                    break;
                case "NOT":
                    logicalType = LogicalType.NOT;
                    break;
                case "<<":
                    logicalType = LogicalType.LEFT;
                    break;
                case ">>":
                    logicalType = LogicalType.RIGHT;
                    break;
            }

            ManualLogicInputChanged();
        }

        private void ConversionTypeChanged(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.Content.ToString() == "Decimal")
                outputType = OutputType.Decimal;
            if (rb.Content.ToString() == "Binary")
                outputType = OutputType.Binary;
            if (rb.Content.ToString() == "HEX")
                outputType = OutputType.Hex;
            ConvertInput();
        }
    }
}
