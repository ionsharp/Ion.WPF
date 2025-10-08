﻿using System.Collections.Generic;

namespace Ion.Data;

public class CharArrayTypeConverter : StringTypeConverter<char>
{
    protected override int? Length => null;

    protected override char Convert(string input) => char.Parse(input);

    protected override object Convert(char[] input) => (IReadOnlyCollection<char>)input;
}