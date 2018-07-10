using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GumballTest
{
    public static class EnumerableEx
    {
        /// <summary>
        /// 从集合中的选出K个元素组合数
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> sequences, int k)
        {
            return k == 0 ? new[] { new T[0] } : sequences.SelectMany((e, i) =>
                                                                       sequences.Skip(i + 1)
                                                                               .Combinations(k - 1)
                                                                             .Select(c => (new[] { e }).Concat(c))
                                                                    );
        }
        /// <summary>
        /// 求集合的笛卡尔积
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Cartesian<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> tempProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(tempProduct,
                                         (accumulator, sequence) =>
                                            from accseq in accumulator
                                            from item in sequence
                                            select accseq.Concat(new[] { item })
                                      );
        }

    }
}
