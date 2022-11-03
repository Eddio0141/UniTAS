//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from MovieScriptDefaultGrammar.g4 by ANTLR 4.9.3

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="MovieScriptDefaultGrammarParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.3")]
[System.CLSCompliant(false)]
public interface IMovieScriptDefaultGrammarListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterProgram([NotNull] MovieScriptDefaultGrammarParser.ProgramContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitProgram([NotNull] MovieScriptDefaultGrammarParser.ProgramContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.actionSeparator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterActionSeparator([NotNull] MovieScriptDefaultGrammarParser.ActionSeparatorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.actionSeparator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitActionSeparator([NotNull] MovieScriptDefaultGrammarParser.ActionSeparatorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.action"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAction([NotNull] MovieScriptDefaultGrammarParser.ActionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.action"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAction([NotNull] MovieScriptDefaultGrammarParser.ActionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.scopeOpen"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterScopeOpen([NotNull] MovieScriptDefaultGrammarParser.ScopeOpenContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.scopeOpen"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitScopeOpen([NotNull] MovieScriptDefaultGrammarParser.ScopeOpenContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.scopeClose"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterScopeClose([NotNull] MovieScriptDefaultGrammarParser.ScopeCloseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.scopeClose"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitScopeClose([NotNull] MovieScriptDefaultGrammarParser.ScopeCloseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.actionWithSeparator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterActionWithSeparator([NotNull] MovieScriptDefaultGrammarParser.ActionWithSeparatorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.actionWithSeparator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitActionWithSeparator([NotNull] MovieScriptDefaultGrammarParser.ActionWithSeparatorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.frameAdvance"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFrameAdvance([NotNull] MovieScriptDefaultGrammarParser.FrameAdvanceContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.frameAdvance"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFrameAdvance([NotNull] MovieScriptDefaultGrammarParser.FrameAdvanceContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.breakAction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBreakAction([NotNull] MovieScriptDefaultGrammarParser.BreakActionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.breakAction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBreakAction([NotNull] MovieScriptDefaultGrammarParser.BreakActionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.continueAction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterContinueAction([NotNull] MovieScriptDefaultGrammarParser.ContinueActionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.continueAction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitContinueAction([NotNull] MovieScriptDefaultGrammarParser.ContinueActionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.returnAction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReturnAction([NotNull] MovieScriptDefaultGrammarParser.ReturnActionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.returnAction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReturnAction([NotNull] MovieScriptDefaultGrammarParser.ReturnActionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.variable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariable([NotNull] MovieScriptDefaultGrammarParser.VariableContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.variable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariable([NotNull] MovieScriptDefaultGrammarParser.VariableContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.variableAssignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableAssignment([NotNull] MovieScriptDefaultGrammarParser.VariableAssignmentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.variableAssignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableAssignment([NotNull] MovieScriptDefaultGrammarParser.VariableAssignmentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.variableTupleSeparation"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableTupleSeparation([NotNull] MovieScriptDefaultGrammarParser.VariableTupleSeparationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.variableTupleSeparation"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableTupleSeparation([NotNull] MovieScriptDefaultGrammarParser.VariableTupleSeparationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.tupleExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTupleExpression([NotNull] MovieScriptDefaultGrammarParser.TupleExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.tupleExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTupleExpression([NotNull] MovieScriptDefaultGrammarParser.TupleExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpression([NotNull] MovieScriptDefaultGrammarParser.ExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpression([NotNull] MovieScriptDefaultGrammarParser.ExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.expressionTerminator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpressionTerminator([NotNull] MovieScriptDefaultGrammarParser.ExpressionTerminatorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.expressionTerminator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpressionTerminator([NotNull] MovieScriptDefaultGrammarParser.ExpressionTerminatorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.string"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterString([NotNull] MovieScriptDefaultGrammarParser.StringContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.string"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitString([NotNull] MovieScriptDefaultGrammarParser.StringContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.intType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIntType([NotNull] MovieScriptDefaultGrammarParser.IntTypeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.intType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIntType([NotNull] MovieScriptDefaultGrammarParser.IntTypeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.floatType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFloatType([NotNull] MovieScriptDefaultGrammarParser.FloatTypeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.floatType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFloatType([NotNull] MovieScriptDefaultGrammarParser.FloatTypeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.bool"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBool([NotNull] MovieScriptDefaultGrammarParser.BoolContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.bool"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBool([NotNull] MovieScriptDefaultGrammarParser.BoolContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.ifElse"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIfElse([NotNull] MovieScriptDefaultGrammarParser.IfElseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.ifElse"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIfElse([NotNull] MovieScriptDefaultGrammarParser.IfElseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodCall([NotNull] MovieScriptDefaultGrammarParser.MethodCallContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodCall([NotNull] MovieScriptDefaultGrammarParser.MethodCallContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodCallArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodCallArgs([NotNull] MovieScriptDefaultGrammarParser.MethodCallArgsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodCallArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodCallArgs([NotNull] MovieScriptDefaultGrammarParser.MethodCallArgsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodDef([NotNull] MovieScriptDefaultGrammarParser.MethodDefContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodDef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodDef([NotNull] MovieScriptDefaultGrammarParser.MethodDefContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodDefArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodDefArgs([NotNull] MovieScriptDefaultGrammarParser.MethodDefArgsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.methodDefArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodDefArgs([NotNull] MovieScriptDefaultGrammarParser.MethodDefArgsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.loop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLoop([NotNull] MovieScriptDefaultGrammarParser.LoopContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MovieScriptDefaultGrammarParser.loop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLoop([NotNull] MovieScriptDefaultGrammarParser.LoopContext context);
}
