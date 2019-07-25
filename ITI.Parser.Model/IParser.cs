using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Parser
{
    public interface IParser
    {
        T Parse<T>(string json);
        string RemoveWhitespace(string json, bool appendEscapeCharacter);
        int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json);
        object ParseValue(Type type, string json);
        bool IsHexStructure(string json, int idx);
        string GetRidOfQuotesMarks(string json);
        bool CheckItIsDictionary(Type keyType, string json);
        List<string> SplitCollection(string json);
    }
}

