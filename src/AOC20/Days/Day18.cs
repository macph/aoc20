using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AOC20.Day18
{
    public class Day18 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day18.txt";

        public uint Day => 18;

        public string Title => "Operation Order";

        public object SolvePart1()
        {
            using var stream = Utils.OpenResource(Resource);
            // Equal precedence for addition and multiplication.
            var operators = new Dictionary<Token, int>
            {
                { Token.Add, 1 },
                { Token.Multiply, 1 },
            };
            return EvaluateExpressions(operators).Sum();
        }

        public object SolvePart2()
        {
            // Addition has higher precedence than multiplication.
            var operators = new Dictionary<Token, int>
            {
                { Token.Add, 2 },
                { Token.Multiply, 1 },
            };
            return EvaluateExpressions(operators).Sum();
        }

        private IEnumerable<long> EvaluateExpressions(Dictionary<Token, int> operators)
        {
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                yield return EvaluateExpression(line, operators);
            }
        }

        private long EvaluateExpression(string input, Dictionary<Token, int> operators)
        {
            // Use the shunting yard algorithm to convert operations from infix into postfix.
            var stack = new Stack<Token>();
            var output = new Stack<object>();

            ReadOnlySpan<char> source = input;

            while (ReadTokens(ref source, out var obj))
            {
                if (obj is long integer)
                {
                    // Move integer to output.
                    output.Push(integer);
                    continue;
                }
                else if (!(obj is Token))
                {
                    throw new Exception($"Expected integer or token, got '{obj}'");
                }

                var token = (Token)obj;
                switch (token)
                {
                    case Token.Left:
                        // New set of parentheses.
                        stack.Push(Token.Left);
                        break;
                    case Token.Right:
                        // Move all operators within parentheses into output.
                        while (operators.Count > 0 && stack.Peek() != Token.Left)
                        {
                            EvaluateInPlace(output, stack.Pop());
                        }
                        if (operators.Count == 0)
                        {
                            throw new Exception("Mismatched parentheses: no '(' found");
                        }
                        // Remove left parenthesis.
                        stack.Pop();
                        break;
                    default:
                        if (!operators.ContainsKey(token))
                        {
                            throw new Exception($"Token does not exist in list of operators");
                        }
                        // Assume all operators are left associative and evaluated left-to-right,
                        // such that operators of equal precedence are moved into the output before
                        // this operator is added to the stack.
                        while (stack.Count > 0 &&
                            stack.Peek() != Token.Left &&
                            operators[stack.Peek()] >= operators[token])
                        {
                            EvaluateInPlace(output, stack.Pop());
                        }
                        stack.Push(token);
                        break;
                }
            }

            // Move all operators into output once stream of tokens has completed.
            while (stack.Count > 0)
            {
                if (stack.Peek() == Token.Left)
                {
                    throw new Exception("Mismatched parentheses: no ')' found");
                }
                EvaluateInPlace(output, stack.Pop());
            }

            return (long)output.Single();
        }

        private void EvaluateInPlace(Stack<object> output, Token token)
        {
            var count = output.Count;
            if (count >= 2 && output.Pop() is long right && output.Pop() is long left)
            {
                // Replace postfix operation with result in place.
                output.Push(EvaluateWithToken(token, left, right));
            }
            else
            {
                throw new Exception("Expected at least two integers");
            }
        }

        private bool ReadTokens(
            ref ReadOnlySpan<char> remaining,
            [MaybeNullWhen(false)] out object token)
        {
            if (remaining.IsEmpty || remaining.IsWhiteSpace())
            {
                token = null;
                return false;
            }

            remaining = remaining.TrimStart();

            var i = 0;
            while (i < remaining.Length && char.IsDigit(remaining[i]))
            {
                i++;
            }

            if (i > 0)
            {
                token = long.Parse(remaining[..i]);
                remaining = remaining[i..];
            }
            else
            {
                token = ParseToken(remaining[0]);
                remaining = remaining[1..];
            }

            return true;
        }

        private Token ParseToken(char c) => c switch
        {
            '+' => Token.Add,
            '*' => Token.Multiply,
            '(' => Token.Left,
            ')' => Token.Right,
            _ => throw new ArgumentOutOfRangeException(
                nameof(c),
                $"Expected valid token, got '{c}'"),
        };

        private long EvaluateWithToken(Token token, long left, long right) => token switch
        {
            Token.Add => left + right,
            Token.Multiply => left * right,
            _ => throw new ArgumentException(
                $"Expected binary operator, got '{token}'",
                nameof(token)),
        };

        private enum Token : byte
        {
            Add,
            Multiply,
            Left,
            Right,
        }
    }
}
