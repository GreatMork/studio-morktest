using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
using System.Threading;

namespace GumballTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<FeiTing> ftList = new ObservableCollection<FeiTing>();
        public ObservableCollection<FeiTing> FTList
        {
            get { return ftList; }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeFeitings();
        }

        private void InitializeFeitings()
        {
            var logLine = string.Empty;
            this.DataContext = this;
            try 
            {
                var lineList = File.ReadAllLines("FT.txt");
                foreach (var line in lineList)
                {
                    logLine = line;
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("#")) continue;
                    var ls = line.Split(',');
                    ftList.Add(new FeiTing(ls[0],
                        int.Parse(ls[1]), int.Parse(ls[2]),
                        int.Parse(ls[3]), int.Parse(ls[4])));
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message + " " + logLine); 
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addedFire = int.Parse(tbFire.Text);
                var addedArm = int.Parse(tbArm.Text);
                var addedMane = int.Parse(tbMane.Text);
                var addedLuck = int.Parse(tbLuck.Text);
                var ftNumber = int.Parse((FtNumber.SelectedItem as ComboBoxItem).Content.ToString());
                var ftHot = int.Parse((FtHot.SelectedItem as ComboBoxItem).Content.ToString());
                var ftArray = FTList.Where(t=>t.IsChecked).ToArray();
                var sumProperty = new FeiTing("sum", addedFire, addedArm, addedMane, addedLuck);
                if(ftArray.Length < ftNumber)
                {
                    MessageBox.Show("选择数量过少");
                    return;
                }

                // 第一次捕获
                var loadedList = new List<FeiTing>();
                for (int i = 0; i < ftNumber; i++)
                {
                    var mostWanted = GetMostWanted(ftArray.Where(t => !loadedList.Contains(t)), sumProperty);
                    loadedList.Add(mostWanted);
                    sumProperty.Append(mostWanted);
                }

                // 第二次微调
                for (int i = 0; i < 10; i++)
                {
                    var diff = sumProperty.GetAverageDiff();
                    if (diff < 2) break;

                    var mostHated = GetMostHated(loadedList, sumProperty);
                    var restList = ftArray.Where(t => !loadedList.Contains(t));
                    var isHit = false;
                    foreach (var item in restList)
                    {
                        var tmpSumProperty = sumProperty.Clone() as FeiTing;
                        tmpSumProperty.Remove(mostHated);
                        tmpSumProperty.Append(item);
                        
                        var tmpDiff = tmpSumProperty.GetAverageDiff();
                        if (tmpDiff < diff)
                        {
                            loadedList.Remove(mostHated);
                            sumProperty.Remove(mostHated);

                            loadedList.Add(item);
                            sumProperty.Append(item);
                            isHit = true;
                            break;
                        }
                    }

                    if (!isHit) break;
                }

                sumProperty.Name = string.Join("、", loadedList.Select(t => t.Name));
                var msg = string.Format("最佳飞艇组合:\n\t{0}\n四维属性:\n\t{1} {2} {3} {4}",
                    sumProperty.Name, 
                    sumProperty.Fire + ftHot * ftNumber,
                    sumProperty.Arm + ftHot * ftNumber,
                    sumProperty.Mane + ftHot * ftNumber, 
                    sumProperty.Luck + ftHot * ftNumber);
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }

        private FeiTing GetMostWanted(IEnumerable<FeiTing> list, FeiTing args)
        {
            var orderList = list.OrderByDescending(t => t.GetMostWantedValue(args));
            return orderList.FirstOrDefault();
        }

        private FeiTing GetMostHated(IEnumerable<FeiTing> list, FeiTing args)
        {
            var orderList = list.OrderBy(t => t.GetMostWantedValue(args));
            return orderList.FirstOrDefault();
        }
    }

    public class FeiTing : ICloneable
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public int Fire { get; set; }
        public int Arm { get; set; }
        public int Mane { get; set; }
        public int Luck { get; set; }
        public int Sum { get; set; }

        public int MinValue { get; set; }

        public FeiTing(string name, int f, int a, int m, int l)
        {
            this.Name = name;
            this.Fire = f;
            this.Arm = a;
            this.Mane = m;
            this.Luck = l;
            this.Sum = f + a + m + l;
            this.MinValue = Math.Min(Math.Min(Fire, Arm), Math.Min(Mane, Luck));
            this.IsChecked = true;
        }

        internal void Append(FeiTing mostWanted)
        {
            this.Fire += mostWanted.Fire;
            this.Arm += mostWanted.Arm;
            this.Mane += mostWanted.Mane;
            this.Luck += mostWanted.Luck;
        }

        internal void Remove(FeiTing mostHated)
        {
            this.Fire -= mostHated.Fire;
            this.Arm -= mostHated.Arm;
            this.Mane -= mostHated.Mane;
            this.Luck -= mostHated.Luck;
        }

        internal int GetMostWantedValue(FeiTing args)
        {
            var items = new SortItem[]
            {
                new SortItem(args.Fire, 0),
                new SortItem(args.Arm, 0),
                new SortItem(args.Mane, 0),
                new SortItem(args.Luck, 0)
            };
            var sortItems = items.OrderBy(t => t.Val).ToArray();
            sortItems[0].Weight = 1;

            return this.Fire * items[0].Weight + this.Arm * items[1].Weight
                + this.Mane * items[2].Weight + this.Luck * items[3].Weight;
        }

        internal int GetAverageDiff()
        {
            int sum = Fire + Arm + Mane + Luck;
            int max = Math.Max(Math.Max(Fire, Arm), Math.Max(Mane, Luck));
            int min = Math.Min(Math.Min(Fire, Arm), Math.Min(Mane, Luck));
            return Math.Abs((sum - max) / 3 - min);
        }



        public object Clone()
        {
            return new FeiTing(Name, Fire, Arm, Mane, Luck);
        }
    }

    public class PermutationAndCombination<T>
    {
        /// <summary>
        /// 交换两个变量
        /// </summary>
        /// <param name="a">变量1</param>
        /// <param name="b">变量2</param>
        public static void Swap(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
        /// <summary>
        /// 递归算法求数组的组合(私有成员)
        /// </summary>
        /// <param name="list">返回的范型</param>
        /// <param name="t">所求数组</param>
        /// <param name="n">辅助变量</param>
        /// <param name="m">辅助变量</param>
        /// <param name="b">辅助数组</param>
        /// <param name="M">辅助变量M</param>
        private static void GetCombination(ref List<T[]> list, T[] t, int n, int m, int[] b, int M)
        {
            for (int i = n; i >= m; i--)
            {
                b[m - 1] = i - 1;
                if (m > 1)
                {
                    GetCombination(ref list, t, i - 1, m - 1, b, M);
                }
                else
                {
                    if (list == null)
                    {
                        list = new List<T[]>();
                    }
                    T[] temp = new T[M];
                    for (int j = 0; j < b.Length; j++)
                    {
                        temp[j] = t[b[j]];
                    }
                    list.Add(temp);
                }
            }
        }
        /// <summary>
        /// 递归算法求排列(私有成员)
        /// </summary>
        /// <param name="list">返回的列表</param>
        /// <param name="t">所求数组</param>
        /// <param name="startIndex">起始标号</param>
        /// <param name="endIndex">结束标号</param>
        private static void GetPermutation(ref List<T[]> list, T[] t, int startIndex, int endIndex)
        {
            if (startIndex == endIndex)
            {
                if (list == null)
                {
                    list = new List<T[]>();
                }
                T[] temp = new T[t.Length];
                t.CopyTo(temp, 0);
                list.Add(temp);
            }
            else
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    Swap(ref t[startIndex], ref t[i]);
                    GetPermutation(ref list, t, startIndex + 1, endIndex);
                    Swap(ref t[startIndex], ref t[i]);
                }
            }
        }
        /// <summary>
        /// 求从起始标号到结束标号的排列，其余元素不变
        /// </summary>
        /// <param name="t">所求数组</param>
        /// <param name="startIndex">起始标号</param>
        /// <param name="endIndex">结束标号</param>
        /// <returns>从起始标号到结束标号排列的范型</returns>
        public static List<T[]> GetPermutation(T[] t, int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex > t.Length - 1)
            {
                return null;
            }
            List<T[]> list = new List<T[]>();
            GetPermutation(ref list, t, startIndex, endIndex);
            return list;
        }
        /// <summary>
        /// 返回数组所有元素的全排列
        /// </summary>
        /// <param name="t">所求数组</param>
        /// <returns>全排列的范型</returns>
        public static List<T[]> GetPermutation(T[] t)
        {
            return GetPermutation(t, 0, t.Length - 1);
        }
        /// <summary>
        /// 求数组中n个元素的排列
        /// </summary>
        /// <param name="t">所求数组</param>
        /// <param name="n">元素个数</param>
        /// <returns>数组中n个元素的排列</returns>
        public static List<T[]> GetPermutation(T[] t, int n)
        {
            if (n > t.Length)
            {
                return null;
            }
            List<T[]> list = new List<T[]>();
            List<T[]> c = GetCombination(t, n);
            for (int i = 0; i < c.Count; i++)
            {
                List<T[]> l = new List<T[]>();
                GetPermutation(ref l, c[i], 0, n - 1);
                list.AddRange(l);
            }
            return list;
        }
        /// <summary>
        /// 求数组中n个元素的组合
        /// </summary>
        /// <param name="t">所求数组</param>
        /// <param name="n">元素个数</param>
        /// <returns>数组中n个元素的组合的范型</returns>
        public static List<T[]> GetCombination(T[] t, int n)
        {
            if (t.Length < n)
            {
                return null;
            }
            int[] temp = new int[n];
            List<T[]> list = new List<T[]>();
            GetCombination(ref list, t, t.Length, n, temp, n);
            return list;
        }
    }

    public class SortItem
    {
        public int Val { get; set; }
        public int Weight { get; set; }

        public SortItem(int val, int sm)
        {
            this.Val = val;
            this.Weight = sm;
        }
    }
}
