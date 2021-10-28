﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rpm.Various;

namespace Rpm.Misc
{
    [Serializable]
    public class FilterTypeInstance
    {
        public Operand OperandType { get; set; }
        public FilterType FilterType { get; set; }
    }

    [Serializable]
    public class FilterEntry
    {
        public int ID { get; set; }
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public bool CompareWithProperty { get; set; }
        public FilterType FilterType { get; set; }
        private Operand _fOperand;
        public Operand OldOperandType { get; set; } = Operand.None;
        public List<FilterEntry> ChildEntries { get; set; }
        public int RangeValue { get; set; }
        public Operand OperandType
        {
            get => _fOperand;
            set
            {
                _fOperand = value;
                if (OldOperandType == Operand.None)
                    OldOperandType = value;
            }
        }

        public string Criteria => ToString();

        public FilterEntry()
        {
            ID = DateTime.Now.GetHashCode();
        }

        public bool ContainsFilter(object instance)
        {
            try
            {
                if (instance == null || Value == null) return false;
                object xvalue = null;
                if (CompareWithProperty && Value is string xpropname && !string.IsNullOrEmpty(xpropname))
                {
                    xvalue = instance.GetPropValue(xpropname);
                }
                var propertyvalue =
                    string.IsNullOrEmpty(PropertyName) ? instance : instance.GetPropValue(PropertyName);
                if (propertyvalue == null) return false;
                var xreturn = ContainsFilter(xvalue??Value, propertyvalue, FilterType);
                if (ChildEntries is {Count: > 0})
                    foreach (var filter in ChildEntries)
                        switch (filter.OperandType)
                        {
                            case Operand.None:
                                continue;
                            case Operand.ALS:
                                if (filter.ContainsFilter(instance)) return true;
                                continue;
                            case Operand.OF:
                                xreturn |= filter.ContainsFilter(instance);
                                continue;
                            case Operand.EN:
                                xreturn &= filter.ContainsFilter(instance);
                                continue;
                        }

                return xreturn;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private bool ContainsFilter(object valueA, object valueB, FilterType type)
        {
            try
            {
                if (valueA == null || valueB == null) return false;
                var xisfilter = false;
                switch (valueB)
                {
                    case string xvalue:
                        if (valueA is not string value0) return false;
                        switch (type)
                        {
                            case FilterType.GelijkAan:
                                xisfilter = string.Equals(xvalue, value0,
                                    StringComparison.CurrentCultureIgnoreCase);
                                break;
                            case FilterType.Bevat:
                                xisfilter = xvalue.ToLower().Contains(value0.ToLower());
                                break;
                            case FilterType.Lager:
                                if (int.TryParse(value0, out var valueint) && int.TryParse(xvalue, out var xvalueint))
                                    xisfilter = xvalueint < valueint;
                                break;
                            case FilterType.LagerOfGelijkAan:
                                if (int.TryParse(value0, out var valueint2) && int.TryParse(xvalue, out var xvalueint2))
                                    xisfilter = xvalueint2 <= valueint2;
                                break;
                            case FilterType.Hoger:
                                if (int.TryParse(value0, out var valueint1) && int.TryParse(xvalue, out var xvalueint1))
                                    xisfilter = xvalueint1 > valueint1;
                                break;
                            case FilterType.HogerOfGelijkAan:
                                if (int.TryParse(value0, out var valueint3) && int.TryParse(xvalue, out var xvalueint3))
                                    xisfilter = xvalueint3 >= valueint3;
                                break;
                            case FilterType.NietGelijkAan:
                                xisfilter = !string.Equals(xvalue, value0,
                                    StringComparison.CurrentCultureIgnoreCase);
                                break;
                            case FilterType.BevatNiet:
                                xisfilter = !xvalue.ToLower().Contains(value0.ToLower());
                                break;
                        }

                        break;
                    case int xvalue:
                        if (!int.TryParse(valueA.ToString(), out var value1)) return false;
                        switch (type)
                        {
                            case FilterType.GelijkAan:
                                xisfilter = RangeValue + value1 == xvalue;
                                break;
                            case FilterType.Bevat:
                                xisfilter = xvalue.ToString().Contains((value1 + RangeValue).ToString());
                                break;
                            case FilterType.Lager:
                                xisfilter = xvalue < RangeValue + value1;
                                break;
                            case FilterType.LagerOfGelijkAan:
                                xisfilter = xvalue <= RangeValue + value1;
                                break;
                            case FilterType.Hoger:
                                xisfilter = xvalue > RangeValue + value1;
                                break;
                            case FilterType.HogerOfGelijkAan:
                                xisfilter = xvalue >= RangeValue + value1;
                                break;
                            case FilterType.NietGelijkAan:
                                xisfilter = RangeValue + value1 != xvalue;
                                break;
                            case FilterType.BevatNiet:
                                xisfilter = !xvalue.ToString().Contains(value1.ToString());
                                break;
                        }

                        break;
                    case double xvalue:
                        if (!double.TryParse(valueA.ToString(), out var xdouble)) return false;
                        switch (type)
                        {
                            case FilterType.GelijkAan:
                                xisfilter = xdouble == RangeValue + xvalue;
                                break;
                            case FilterType.Bevat:
                                xisfilter = xvalue.ToString().Contains((xdouble + RangeValue).ToString());
                                break;
                            case FilterType.Lager:
                                xisfilter = xvalue < RangeValue + xdouble;
                                break;
                            case FilterType.LagerOfGelijkAan:
                                xisfilter = xvalue <= RangeValue + xdouble;
                                break;
                            case FilterType.Hoger:
                                xisfilter = xvalue > RangeValue + xdouble;
                                break;
                            case FilterType.HogerOfGelijkAan:
                                xisfilter = xvalue >= RangeValue + xdouble;
                                break;
                            case FilterType.NietGelijkAan:
                                xisfilter = RangeValue + xdouble != xvalue;
                                break;
                            case FilterType.BevatNiet:
                                xisfilter = !xvalue.ToString().Contains((RangeValue + xdouble).ToString());
                                break;
                        }

                        break;
                    case decimal xvalue:
                        if (!decimal.TryParse(valueA.ToString(), out var xdecimal)) return false;
                        switch (type)
                        {
                            case FilterType.GelijkAan:
                                xisfilter = RangeValue + xdecimal == xvalue;
                                break;
                            case FilterType.Bevat:
                                xisfilter = xvalue.ToString().Contains((RangeValue + xdecimal).ToString());
                                break;
                            case FilterType.Lager:
                                xisfilter = xvalue < RangeValue + xdecimal;
                                break;
                            case FilterType.LagerOfGelijkAan:
                                xisfilter = xvalue <= RangeValue + xdecimal;
                                break;
                            case FilterType.Hoger:
                                xisfilter = xvalue > RangeValue + xdecimal;
                                break;
                            case FilterType.HogerOfGelijkAan:
                                xisfilter = xvalue >= RangeValue + xdecimal;
                                break;
                            case FilterType.NietGelijkAan:
                                xisfilter = RangeValue + xdecimal != xvalue;
                                break;
                            case FilterType.BevatNiet:
                                xisfilter = !xvalue.ToString().Contains((RangeValue + xdecimal).ToString());
                                break;
                        }

                        break;
                    case DateTime xvalue:
                        if (valueA is not DateTime value2) return false;
                        if (xvalue.Year == 9999 && xvalue.Month == 1 && xvalue.Day == 1)
                            xvalue = DateTime.Now;
                        if (value2.Year == 9999 && value2.Month == 1 && value2.Day == 1)
                            value2 = DateTime.Now;
                        switch (type)
                        {
                            case FilterType.GelijkAan:
                                xisfilter = value2 == xvalue;
                                break;
                            case FilterType.Bevat:
                                xisfilter = xvalue.ToString().Contains(value2.ToString());
                                break;
                            case FilterType.Lager:
                                xisfilter = xvalue < value2;
                                break;
                            case FilterType.LagerOfGelijkAan:
                                xisfilter = xvalue <= value2;
                                break;
                            case FilterType.Hoger:
                                xisfilter = xvalue > value2;
                                break;
                            case FilterType.HogerOfGelijkAan:
                                xisfilter = xvalue >= value2;
                                break;
                            case FilterType.NietGelijkAan:
                                xisfilter = value2 != xvalue;
                                break;
                            case FilterType.BevatNiet:
                                xisfilter = !xvalue.ToString().Contains(value2.ToString());
                                break;
                        }

                        break;
                    case Enum xvalue:
                        if (valueA is not Enum value3) return false;
                        var xvalueenum = Enum.GetName(xvalue.GetType(), xvalue);
                        var valueenum = Enum.GetName(value3.GetType(), value3);
                        xisfilter = ContainsFilter(valueenum, xvalueenum, type);
                        break;
                    case bool xvalue:
                        if (valueA is not bool value4) return false;
                        switch (type)
                        {
                            case FilterType.GelijkAan:
                                xisfilter = value4 == xvalue;
                                break;
                            case FilterType.Bevat:
                                xisfilter = xvalue.ToString().Contains(value4.ToString());
                                break;
                            case FilterType.LagerOfGelijkAan:
                            case FilterType.HogerOfGelijkAan:
                            case FilterType.Lager:
                            case FilterType.Hoger:
                                xisfilter = value4 != xvalue;
                                break;

                            case FilterType.NietGelijkAan:
                                xisfilter = value4 != xvalue;
                                break;
                            case FilterType.BevatNiet:
                                xisfilter = !xvalue.ToString().Contains(value4.ToString());
                                break;
                        }

                        break;
                }

                return xisfilter;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public string ValueToString()
        {
            try
            {
                if (Value == null) return null;
                switch (Value)
                {
                    case string xvalue:
                        return xvalue.ToString(CultureInfo.InvariantCulture);
                    case decimal xvalue:
                        return xvalue.ToString(CultureInfo.InvariantCulture);
                    case double xvalue:
                        return xvalue.ToString(CultureInfo.InvariantCulture);
                    case int xvalue:
                        return xvalue.ToString(CultureInfo.InvariantCulture);
                    case DateTime xvalue:
                        if (xvalue.Year == 9999 && xvalue.Month == 1 && xvalue.Day == 1)
                            return "'Huidige Tijd'";
                        return xvalue.ToString();
                    case Enum xvalue:
                        var xvalueenum = Enum.GetName(xvalue.GetType(), xvalue);
                        return xvalueenum;
                    case bool xvalue:
                        return xvalue ? "WAAR" : "NIETWAAR";
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private string GetRangeValueString()
        {
            return (RangeValue != 0 ? RangeValue > 0 ? $" (+{RangeValue})" : $" ({RangeValue})" : "");
        }

        public new string ToString()
        {
            try
            {
                var value = ValueToString();
                var xreturn =
                    $"{Enum.GetName(typeof(Operand), OperandType)} '{PropertyName}' {Enum.GetName(typeof(FilterType), FilterType)} '{value}' {GetRangeValueString()}";
                if (ChildEntries is {Count: > 0})
                    xreturn += $"\n({string.Join("\n", ChildEntries.Select(x => x.ToString()))})";
                return xreturn;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }

        public string ToHtmlString()
        {
            try
            {
                var value = ValueToString();
                var xreturn =
                    $"<div>(<span Color=RoyalBlue>{Enum.GetName(typeof(Operand), OperandType)}</span> <span Color=Purple>{PropertyName}</span> <span Color=RoyalBlue>{Enum.GetName(typeof(FilterType), FilterType)}</span><span Color=Purple> {value}</span><span Color=Red> {GetRangeValueString()}</span>)</div>";
                if (ChildEntries is {Count: > 0})
                    xreturn += "\n" + string.Join("\n", ChildEntries.Select(x => x.ToHtmlString()));
                return xreturn;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is FilterEntry entry && entry.ID == ID;
        }

        protected bool Equals(FilterEntry other)
        {
            return ID == other.ID;
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}