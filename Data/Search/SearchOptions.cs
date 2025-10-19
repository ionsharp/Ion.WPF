using Ion.Core;
using System;

namespace Ion.Data;

public record class SearchOptions : Model
{
    public bool Case { get => Get(false); set => Set(value); }

    public SearchCondition Condition { get => Get(SearchCondition.StartsWith); set => Set(value); }

    public SearchWord Word { get => Get(SearchWord.Exact); set => Set(value); }

    public SearchOptions() : base() { }

    public bool Assert(string a, string b)
    {
        if (!Case)
        {
            a = a.ToLower();
            b = b.ToLower();
        }

        switch (Word)
        {
            case SearchWord.All:
            case SearchWord.Any:
            case SearchWord.None:

                var words = b.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                switch (Word)
                {
                    case SearchWord.All:
                        switch (Condition)
                        {
                            case SearchCondition.Contains:
                                foreach (var i in words)
                                {
                                    if (!a.Contains(i))
                                        return false;
                                }
                                return true;

                            case SearchCondition.EndsWith:
                                foreach (var i in words)
                                {
                                    if (!a.EndsWith(i))
                                        return false;
                                }
                                return true;

                            case SearchCondition.StartsWith:
                                foreach (var i in words)
                                {
                                    if (!a.StartsWith(i))
                                        return false;
                                }
                                return true;
                        }
                        break;

                    case SearchWord.Any:
                        switch (Condition)
                        {
                            case SearchCondition.Contains:
                                foreach (var i in words)
                                {
                                    if (a.Contains(i))
                                        return true;
                                }
                                return false;

                            case SearchCondition.EndsWith:
                                foreach (var i in words)
                                {
                                    if (a.EndsWith(i))
                                        return true;
                                }
                                return false;

                            case SearchCondition.StartsWith:
                                foreach (var i in words)
                                {
                                    if (a.StartsWith(i))
                                        return true;
                                }
                                return false;
                        }
                        break;

                    case SearchWord.None:
                        switch (Condition)
                        {
                            case SearchCondition.Contains:
                                foreach (var i in words)
                                {
                                    if (a.Contains(i))
                                        return false;
                                }
                                return true;

                            case SearchCondition.EndsWith:
                                foreach (var i in words)
                                {
                                    if (a.EndsWith(i))
                                        return false;
                                }
                                return true;

                            case SearchCondition.StartsWith:
                                foreach (var i in words)
                                {
                                    if (a.StartsWith(i))
                                        return false;
                                }
                                return true;
                        }
                        break;
                }

                break;

            case SearchWord.Exact:
                switch (Condition)
                {
                    case SearchCondition.Contains:
                        return a.Contains(b);

                    case SearchCondition.EndsWith:
                        return a.EndsWith(b);

                    case SearchCondition.StartsWith:
                        return a.StartsWith(b);
                }
                break;
        }

        return false;
    }
}