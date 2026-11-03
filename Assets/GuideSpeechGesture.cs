using System.Text.RegularExpressions;
public static class GuideSpeechGesture {
 public static string FromWords(string words){var s=(words??"").ToLowerInvariant();if(Regex.IsMatch(s,@"\b(hello|welcome|goodbye)\b"))return "wave";if(Regex.IsMatch(s,@"\b(not specified|does not say|uncertain|don't know|do not know)\b"))return "shrug";if(Regex.IsMatch(s,@"\b(yes|exactly|that's right)\b"))return "nod";if(Regex.IsMatch(s,@"\b(no|never|forbidden)\b"))return "shake";if(Regex.IsMatch(s,@"\b(here|look|to the north|to the south|before the veil)\b"))return "point";if(Regex.IsMatch(s,@"\b(made|built|washed|burned|placed|priest|bread|lamp|ark|altar)\b"))return "explain";return "beat";}
}
