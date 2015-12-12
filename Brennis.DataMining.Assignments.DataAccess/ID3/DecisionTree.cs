using Brennis.DataMining.Assignments.Common;
using Brennis.DataMining.Assignments.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Brennis.DataMining.Assignments.DataAccess.ID3
{
    public class Attribute
    {
        private readonly ArrayList _values;
        private readonly object _label;

        public Attribute(string name, string[] values)
        {
            AttributeName = name;
            _values = new ArrayList(values);
            _values.Sort();
        }

        public Attribute(object label)
        {
            _label = label;
            AttributeName = string.Empty;
            _values = null;
        }

        public string AttributeName { get; }

        public string[] Values => (string[])_values?.ToArray(typeof(string));

        public bool IsValidValue(string value)
        {
            return IndexValue(value) >= 0;
        }

        public int IndexValue(string value)
        {
            return _values?.BinarySearch(value) ?? -1;
        }

        public override string ToString()
        {
            return AttributeName != string.Empty ? AttributeName : _label.ToString();
        }
    }

    public class TreeNode
    {
        private readonly ArrayList _childs;

        public TreeNode(Attribute attribute)
        {
            if (attribute.Values != null)
            {
                _childs = new ArrayList(attribute.Values.Length);
                for (int i = 0; i < attribute.Values.Length; i++)
                    // ReSharper disable once AssignNullToNotNullAttribute
                    _childs.Add(null);
            }
            else
                _childs = new ArrayList(1) { null };

            Attribute = attribute;
        }

        public void AddTreeNode(TreeNode treeNode, string valueName)
        {
            int index = Attribute.IndexValue(valueName);
            _childs[index] = treeNode;
        }

        public int TotalChilds => _childs.Count;

        public TreeNode GetChild(int index)
        {
            return (TreeNode)_childs[index];
        }

        public Attribute Attribute { get; }

        public TreeNode GetChildByBranchName(string branchName)
        {
            return (TreeNode)_childs[Attribute.IndexValue(branchName)];
        }
    }

    public class DecisionTreeId3
    {
        private DataTable _samples;
        private int _totalPositives;
        private int _total;
        private string _targetAttribute;

        private double _mEntropySet;

        private int CountTotalPositives(DataTable samples)
        {
            return samples.Rows.Cast<DataRow>().Count(aRow => aRow[_targetAttribute].ToString().Equals("yes"));
        }

        private static double CalcEntropy(int positives, int negatives)
        {
            int total = positives + negatives;
            double ratioPositive = (double)positives / total;
            double ratioNegative = (double)negatives / total;

            if (Math.Abs(ratioPositive) > 0.1)
                ratioPositive = -(ratioPositive) * Math.Log(ratioPositive, 2);
            if (Math.Abs(ratioNegative) > 0.1)
                ratioNegative = -(ratioNegative) * Math.Log(ratioNegative, 2);

            return ratioPositive + ratioNegative;
        }

        private void GetValuesToAttribute(DataTable samples, Attribute attribute, string value, out int positives,
            out int negatives)
        {
            positives = 0;
            negatives = 0;

            foreach (
                DataRow aRow in
                    samples.Rows.Cast<DataRow>().Where(aRow => (string)aRow[attribute.AttributeName] == value))
            {
                if (aRow[_targetAttribute].ToString().Equals("yes"))
                    positives++;
                else
                    negatives++;
            }
        }

        private double Gain(DataTable samples, Attribute attribute)
        {
            string[] values = attribute.Values;
            double sum = 0.0;

            foreach (string value in values)
            {
                int negatives, positives;

                GetValuesToAttribute(samples, attribute, value, out positives, out negatives);

                double entropy = CalcEntropy(positives, negatives);
                sum += -(double)(positives + negatives) / _total * entropy;
            }

            return _mEntropySet + sum;
        }

        private Attribute GetBestAttribute(DataTable samples, IEnumerable<Attribute> attributes)
        {
            double maxGain = 0.0;
            Attribute result = null;

            foreach (Attribute attribute in attributes)
            {
                double aux = Gain(samples, attribute);
                if (!(aux > maxGain)) continue;

                maxGain = aux;
                result = attribute;
            }

            return result;
        }

        private static bool AllSamplesPositives(DataTable samples, string targetAttribute)
        {
            return samples.Rows.Cast<DataRow>().All(row => row[targetAttribute].ToString().Equals("yes"));
        }

        private static bool AllSamplesNegatives(DataTable samples, string targetAttribute)
        {
            return samples.Rows.Cast<DataRow>().All(row => row[targetAttribute].ToString().Equals("no"));
        }

        private static ArrayList GetDistinctValues(DataTable samples, string targetAttribute)
        {
            ArrayList distinctValues = new ArrayList(samples.Rows.Count);

            foreach (
                DataRow row in
                    samples.Rows.Cast<DataRow>().Where(row => distinctValues.IndexOf(row[targetAttribute]) == -1))
                distinctValues.Add(row[targetAttribute]);

            return distinctValues;
        }

        private static object GetMostCommonValue(DataTable samples, string targetAttribute)
        {
            ArrayList distinctValues = GetDistinctValues(samples, targetAttribute);
            int[] count = new int[distinctValues.Count];

            foreach (int index in from DataRow row in samples.Rows select distinctValues.IndexOf(row[targetAttribute]))
                count[index]++;

            int maxIndex = 0;
            int maxCount = 0;

            for (int i = 0; i < count.Length; i++)
            {
                if (count[i] <= maxCount) continue;
                maxCount = count[i];
                maxIndex = i;
            }

            return distinctValues[maxIndex];
        }

        private TreeNode InternalMountTree(DataTable samples, string targetAttribute,
            IReadOnlyCollection<Attribute> attributes)
        {
            if (AllSamplesPositives(samples, targetAttribute))
                return new TreeNode(new Attribute(true));

            if (AllSamplesNegatives(samples, targetAttribute))
                return new TreeNode(new Attribute(false));

            if (attributes.Count == 0)
                return new TreeNode(new Attribute(GetMostCommonValue(samples, targetAttribute)));

            _total = samples.Rows.Count;
            _targetAttribute = targetAttribute;
            _totalPositives = CountTotalPositives(samples);

            _mEntropySet = CalcEntropy(_totalPositives, _total - _totalPositives);

            Attribute bestAttribute = GetBestAttribute(samples, attributes);

            TreeNode root = new TreeNode(bestAttribute);

            DataTable aSample = samples.Clone();

            foreach (string value in bestAttribute.Values)
            {
                // Select all the elements with this Attribute value
                aSample.Rows.Clear();

                DataRow[] rows = samples.Select(bestAttribute.AttributeName + " = " + "'" + value + "'");

                foreach (DataRow row in rows)
                    aSample.Rows.Add(row.ItemArray);

                // Select all the elements with this Attribute value

                // Create a new list of attributes less the current Attribute that is the best Attribute
                ArrayList aAttributes = new ArrayList(attributes.Count - 1);
                foreach (
                    Attribute attribute in
                        attributes.Where(attribute => attribute.AttributeName != bestAttribute.AttributeName))
                    aAttributes.Add(attribute);

                // Create a new list of attributes less the current Attribute that is the best Attribute

                if (aSample.Rows.Count == 0)
                    return new TreeNode(new Attribute(GetMostCommonValue(aSample, targetAttribute)));

                DecisionTreeId3 dc3 = new DecisionTreeId3();
                TreeNode childNode = dc3.MountTree(aSample, targetAttribute,
                    (Attribute[])aAttributes.ToArray(typeof(Attribute)));
                root.AddTreeNode(childNode, value);
            }

            return root;
        }

        public TreeNode MountTree(DataTable samples, string targetAttribute, Attribute[] attributes)
        {
            _samples = samples;
            return InternalMountTree(_samples, targetAttribute, attributes);
        }
    }

    public class DecisionTree : IDecisionTree
    {
        private static void PrintNode(TreeNode root, string tabs)
        {
            Console.WriteLine(tabs + '|' + root.Attribute + '|');

            if (root.Attribute.Values == null) return;
            foreach (string t in root.Attribute.Values)
            {
                Console.WriteLine(tabs + "\t" + "<" + t + ">");
                TreeNode childNode = root.GetChildByBranchName(t);
                PrintNode(childNode, "\t" + tabs);
            }
        }

        public void Process()
        {
            Attribute[] attributes =
                StaticStorage.DataSet.Columns.Cast<DataColumn>().Where(m => !m.ColumnName.Equals(StaticStorage.TargetColum))
                    .Select(
                        m =>
                            new Attribute(m.ColumnName,
                                StaticStorage.DataSet.AsEnumerable()
                                    .Select(i => i[m.ColumnName].ToString().Format())
                                    .ToArray()
                                    .Distinct()
                                    .ToArray())).ToArray();

            DecisionTreeId3 id3 = new DecisionTreeId3();
            TreeNode root = id3.MountTree(StaticStorage.DataSet, StaticStorage.TargetColum, attributes);

            PrintNode(root, "");
        }
    }
}