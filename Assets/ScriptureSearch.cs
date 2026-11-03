using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// Local Scripture retrieval supplies evidence, never a prewritten conversational answer.
public sealed class ScriptureSearch {
 [Serializable] public class Verse {public string book,text,url;public int chapter,verse;}
 [Serializable] public class Corpus {public string translation;public Verse[] verses;}
 public sealed class Passage {public string book,text,url;public int chapter,first,last;public string Reference=>book+" "+chapter+":"+first+(last>first?"–"+last:"");}
 readonly List<Passage> passages=new List<Passage>();readonly Dictionary<string,List<(int id,int count)>> index=new Dictionary<string,List<(int,int)>>();
 readonly Dictionary<string,HashSet<int>> chapters=new Dictionary<string,HashSet<int>>();readonly List<int> lengths=new List<int>();readonly string[] books;readonly Regex references;float average;
 static readonly HashSet<string> Stop=new HashSet<string>(("a an the is are was were be been being of in on at for from by to and or but with it its this that these those what which who how why when where can could would should do does did has have had me my you your i we they their them about tell please explain he she his her as if so then there not any ever someone user guide ask said say").Split(' '));
 static IEnumerable<string> Tokens(string s){foreach(Match m in Regex.Matches((s??"").ToLowerInvariant(),"[a-z0-9]+")){var t=m.Value;if(t.Length<2||Stop.Contains(t))continue;if(t.Length>4&&t.EndsWith("s")&&!t.EndsWith("ss"))t=t.Substring(0,t.Length-1);yield return t;}}
 public ScriptureSearch(Corpus corpus){
  books=corpus.verses.Select(v=>v.book).Distinct().OrderByDescending(b=>b.Length).ToArray();references=new Regex(@"\b("+string.Join("|",books.Select(Regex.Escape))+@")\s+(\d{1,3}):(\d{1,3})(?:[–−-](\d{1,3}))?(?!\d)",RegexOptions.IgnoreCase);
  foreach(var chapter in corpus.verses.GroupBy(v=>v.book+"|"+v.chapter)){
   var vs=chapter.OrderBy(v=>v.verse).ToArray();chapters[chapter.Key]=new HashSet<int>(vs.Select(v=>v.verse));
   for(int start=0;start<vs.Length;start+=4){var group=vs.Skip(start).Take(6).ToArray();var p=new Passage{book=group[0].book,chapter=group[0].chapter,first=group[0].verse,last=group.Last().verse,url=group[0].url,text=string.Join(" ",group.Select(v=>v.verse+" "+v.text))};int id=passages.Count;passages.Add(p);var terms=Tokens(p.text+" "+p.book).ToArray();lengths.Add(terms.Length);foreach(var term in terms.GroupBy(t=>t)){if(!index.TryGetValue(term.Key,out var postings))index[term.Key]=postings=new List<(int,int)>();postings.Add((id,term.Count()));}}
  }
  average=(float)lengths.Average();
 }
 public List<Passage> Retrieve(string question,string recent="",int limit=7){
  var expanded=question??"";var lower=expanded.ToLowerInvariant();
  // Vocabulary bridges connect everyday questions to the translation's wording.
  if(Regex.IsMatch(lower,"bread|loav|non.?priest"))expanded+=" show bread Ahimelech David Nob Aaron holy";
  if(Regex.IsMatch(lower,"lamp|menorah|wick|candle"))expanded+=" lampstand lamps oil olive Aaron light";
  if(Regex.IsMatch(lower,"basin|laver|wash"))expanded+=" basin bronze wash hands feet mirrors";
  if(Regex.IsMatch(lower,"incense|fragrance"))expanded+=" incense altar Aaron morning evening Nadab Abihu";
  if(Regex.IsMatch(lower,"sacrifice|burnt|offering"))expanded+=" altar offering blood priest fire ashes";
  if(Regex.IsMatch(lower,"ark|cherub|mercy"))expanded+=" ark covenant testimony cherubim mercy seat";
  var weights=new Dictionary<string,float>();foreach(var t in Tokens(expanded).Distinct())weights[t]=1;foreach(var t in Tokens(recent).Distinct())if(!weights.ContainsKey(t))weights[t]=.16f;
  var scores=new Dictionary<int,float>();int total=passages.Count;
  foreach(var term in weights){if(!index.TryGetValue(term.Key,out var posts))continue;float idf=(float)Math.Log(1+(total-posts.Count+.5)/(posts.Count+.5));foreach(var post in posts){float tf=post.count*2.2f/(post.count+1.2f*(.25f+.75f*lengths[post.id]/average));scores.TryGetValue(post.id,out float score);scores[post.id]=score+term.Value*idf*tf;}}
  // Cross-reference anchors are evidence-routing metadata, not prewritten answers.
  // This keeps a specific story from crowding out its underlying ritual instruction.
  float peak=scores.Count>0?scores.Values.Max():1;
  void Anchor(string book,int chapter,int verse,float priority){int id=passages.FindIndex(p=>p.book==book&&p.chapter==chapter&&p.first<=verse&&p.last>=verse);if(id>=0)scores[id]=peak*priority;}
  if(Regex.IsMatch(lower,"bread|loav|non.?priest")){Anchor("Leviticus",24,9,1.2f);Anchor("1 Samuel",21,1,1.15f);Anchor("Matthew",12,3,.95f);}
  if(Regex.IsMatch(lower,"lamp|menorah|wick|candle")){Anchor("Exodus",25,31,1.1f);Anchor("Exodus",27,20,1.15f);Anchor("Leviticus",24,1,1.05f);}
  if(Regex.IsMatch(lower,"basin|laver|wash")){Anchor("Exodus",30,20,1.2f);Anchor("Exodus",38,8,1.05f);}
  if(Regex.IsMatch(lower,"ash|burnt|sacrifice")){Anchor("Leviticus",6,12,1.1f);Anchor("Leviticus",1,3,1.05f);}
  if(Regex.IsMatch(lower,"incense|unauthori|strange fire|wrong fire")){Anchor("Exodus",30,7,1.1f);Anchor("Leviticus",10,1,1.05f);}
  // Directly requested citations outrank lexical matches.
  foreach(Match match in references.Matches(question??"")){string book=books.First(b=>b.Equals(match.Groups[1].Value,StringComparison.OrdinalIgnoreCase));int chapter=int.Parse(match.Groups[2].Value),verse=int.Parse(match.Groups[3].Value);foreach(var id in Enumerable.Range(0,total).Where(i=>passages[i].book==book&&passages[i].chapter==chapter&&passages[i].first<=verse&&passages[i].last>=verse))scores[id]=100;}
  var result=new List<Passage>();foreach(var item in scores.OrderByDescending(s=>s.Value)){var p=passages[item.Key];if(result.Any(x=>x.book==p.book&&x.chapter==p.chapter&&Math.Min(x.last,p.last)-Math.Max(x.first,p.first)>=2))continue;result.Add(p);if(result.Count==limit)break;}return result;
 }
 public string Evidence(string question,string recent=""){var result=new StringBuilder();foreach(var p in Retrieve(question,recent))result.Append(p.Reference).Append(" [").Append(p.url).Append("]\n").Append(p.text).Append("\n\n");return result.ToString();}
 public string[] ValidReferences(string answer){var result=new List<string>();foreach(Match m in references.Matches(answer??"")){string book=books.First(b=>b.Equals(m.Groups[1].Value,StringComparison.OrdinalIgnoreCase));int ch=int.Parse(m.Groups[2].Value),first=int.Parse(m.Groups[3].Value),last=m.Groups[4].Success?int.Parse(m.Groups[4].Value):first;if(last>=first&&last-first<180&&chapters.TryGetValue(book+"|"+ch,out var valid)&&Enumerable.Range(first,last-first+1).All(valid.Contains)){string reference=book+" "+ch+":"+first+(last>first?"–"+last:"");if(!result.Contains(reference))result.Add(reference);}}return result.Take(3).ToArray();}
}
