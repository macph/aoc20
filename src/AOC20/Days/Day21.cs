using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day21
{
    public class Day21 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day21.txt";

        public uint Day => 21;

        public string Title => "Allergen Assessment";

        public object SolvePart1()
        {
            var list = ReadFoodList();
            var classified = list.ClassifyIngredients();
            return list
                .SelectMany(f => f.Ingredients)
                .Where(i => classified[i] == string.Empty)
                .Count();
        }

        public object SolvePart2()
        {
            var list = ReadFoodList();
            var dangerous = list.ClassifyIngredients()
                .Where(pair => pair.Value != string.Empty)
                .OrderBy(pair => pair.Value)
                .Select(pair => pair.Key);
            return String.Join(',', dangerous);
        }

        private FoodList ReadFoodList()
        {
            using var stream = Utils.OpenResource(Resource);
            return new FoodList(Utils.ReadLines(stream).Select(Food.Parse));
        }
    }

    public class FoodList : IReadOnlyList<Food>
    {
        private readonly Food[] inner;

        private HashSet<string>? allergens;
        private HashSet<string>? ingredients;

        public FoodList(IEnumerable<Food> foods)
        {
            inner = foods.ToArray();
        }

        public Food this[int index] => inner[index];

        public int Count => inner.Length;

        public IReadOnlyCollection<string> Allergens
        {
            get
            {
                if (allergens is null) BuildSets();
                return allergens!;
            }
        }

        public IReadOnlyCollection<string> Ingredients
        {
            get
            {
                if (ingredients is null) BuildSets();
                return ingredients!;
            }
        }

        private void BuildSets()
        {
            ingredients = new HashSet<string>();
            allergens = new HashSet<string>();

            foreach (Food f in inner)
            {
                ingredients.UnionWith(f.Ingredients);
                allergens.UnionWith(f.Allergens);
            }
        }

        public Dictionary<string, string> ClassifyIngredients()
        {
            var found = Allergens.ToDictionary(a => a, FilterIngredients);

            // If an allergen has only one ingredient after filtering the same ingredient can be
            // removed from other allergens. Repeat until no more ingredients are removed.
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (string a in found.Keys)
                {
                    if (found[a].Count != 1) continue;

                    var i = found[a].First();
                    foreach (string b in found.Keys)
                    {
                        if (a != b)
                        {
                            var thisChanged = found[b].Remove(i);
                            changed = changed || thisChanged;
                        }
                    }
                }
            }

            // Expect 1-1 relationship between ingredients and allergens.
            if (found.Count < Allergens.Count)
            {
                throw new Exception("Not all allergens have had their ingredients identified");
            }
            
            var classified = Ingredients.ToDictionary(i => i, i => string.Empty);

            foreach (KeyValuePair<string, HashSet<string>> pair in found)
            {
                classified[pair.Value.Single()] = pair.Key;
            }

            return classified;
        }

        private HashSet<string> FilterIngredients(string allergen)
        {
            // Find all ingredients in list appearing in the same food items as an allergen.
            var possible = inner
                .Where(f => f.Allergens.Contains(allergen))
                .Select(f => f.Ingredients);
            
            // Start with set of ingredients for first food item found then intersect with others.
            // All other ingredients will not contain this allergen.
            var ingredients = possible.First().ToHashSet();
            foreach (HashSet<string> other in possible.Skip(1))
            {
                ingredients.IntersectWith(other);
            }

            return ingredients;
        }

        public IEnumerator<Food> GetEnumerator() => ((IEnumerable<Food>)inner).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public class Food
    {
        private readonly HashSet<string> ingredients;
        private readonly HashSet<string> allergens;

        public IEnumerable<string> Ingredients => ingredients;

        public IEnumerable<string> Allergens => allergens;

        private Food(string[] ingredients, string[] allergens)
        {
            this.ingredients = ingredients.ToHashSet();
            this.allergens = allergens.ToHashSet();
        }

        public static Food Parse(string input)
        {
            var allergenPrefix = "contains ";
            var start = input.IndexOf('(');
            var end = input.IndexOf(')');
            if (start < 0 ||
                end < 0 ||
                start > end ||
                !input.AsSpan(start + 1).StartsWith(allergenPrefix))
            {
                throw new FormatException("Expected list of allergens at end");
            }

            var ingredients = input[..start].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var allergenStart = start + 1 + allergenPrefix.Length;
            var allergens = input[allergenStart..end].Split(", ");

            return new Food(ingredients, allergens);
        }
    }
}
