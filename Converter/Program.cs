using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Neo;
using Neo.VM;

namespace Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("输入script的base64String:");
                    string base64String = Console.ReadLine();
                    var script = Convert.FromBase64String(base64String);
                    List<Instruction> instructions = new List<Instruction>();
                    Script s = new Script(script, true);
                    for (int ip = 0; ip < s.Length; ip += s.GetInstruction(ip).Size)
                    {
                        var instruction = s.GetInstruction(ip);
                        Console.WriteLine(instruction.OpCode + "        " + instruction.Operand.Span.ToHexString());

                        //if (instruction.OpCode == OpCode.SYSCALL)
                        //{
                        //    var a = Neo.SmartContract.ApplicationEngine.Services[(uint)new BigInteger(instruction.Operand.Span.ToArray())];
                        //    Console.WriteLine(instruction.OpCode);
                        //    Console.WriteLine(instruction.Operand.Span.ToHexString() + "==>" + a.Name);
                        //}
                        //else
                        //{

                        //}

                        instructions.Add(instruction);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
    }
}
