using Mono.Cecil.Cil;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Extensions;

public static class ILProcessorExtensions
{
    /// <summary>
    /// Inserts an instruction before the specified instruction as if it were replacing it.<br/><br/>
    /// This also fixes: <br/>
    /// - Jump targets of any instructions that jump to the replaced instruction.<br/>
    /// - Exception range fixing where if the replaced instruction is in a try block, the new instruction will be added to the try block and vice versa.<br/>
    /// </summary>
    /// <param name="ilProcessor"></param>
    /// <param name="replacingInstruction"></param>
    /// <param name="newInstruction"></param>
    public static void InsertBeforeInstructionReplace(this ILProcessor ilProcessor, Instruction replacingInstruction,
        Instruction newInstruction)
    {
        if (ilProcessor.Body == null)
        {
            StaticLogger.Log.LogWarning("Body is null, skipping");
            return;
        }

        if (replacingInstruction == null || newInstruction == null)
        {
            StaticLogger.Log.LogWarning("Instruction is null, skipping");
            return;
        }

        // insert
        ilProcessor.InsertBefore(replacingInstruction, newInstruction);

        var instructions = ilProcessor.Body.Instructions;

        // fix jump targets
        foreach (var instruction in instructions)
        {
            if (instruction.Operand == replacingInstruction)
            {
                instruction.Operand = newInstruction;
                continue;
            }

            // switch
            if (instruction.Operand is not Instruction[] instructionsArray) continue;

            for (var i = 0; i < instructionsArray.Length; i++)
            {
                if (instructionsArray[i] == replacingInstruction)
                {
                    instructionsArray[i] = newInstruction;
                }
            }
        }

        // fix exception ranges
        if (!ilProcessor.Body.HasExceptionHandlers) return;

        foreach (var exceptionHandler in ilProcessor.Body.ExceptionHandlers)
        {
            // try
            if (exceptionHandler.TryStart == replacingInstruction)
            {
                exceptionHandler.TryStart = newInstruction;
            }

            if (exceptionHandler.TryEnd == replacingInstruction)
            {
                exceptionHandler.TryEnd = newInstruction;
            }

            // catch / finally
            if (exceptionHandler.HandlerStart == replacingInstruction)
            {
                exceptionHandler.HandlerStart = newInstruction;
            }

            if (exceptionHandler.HandlerEnd == replacingInstruction)
            {
                exceptionHandler.HandlerEnd = newInstruction;
            }

            // some special case idk
            if (exceptionHandler.FilterStart == replacingInstruction)
            {
                exceptionHandler.FilterStart = newInstruction;
            }
        }
    }
}