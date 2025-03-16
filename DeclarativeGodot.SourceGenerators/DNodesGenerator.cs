using DeclarativeGodot.SourceGenerators.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DeclarativeGodot.SourceGenerators;

[Generator]
public class DNodesGenerator : IIncrementalGenerator {

	public virtual void Initialize(IncrementalGeneratorInitializationContext igiContext) {

		IncrementalValuesProvider<NormalNode> normalNodeClasses =
			igiContext.CompilationProvider
						.SelectMany((compilation, cancellationToken) => {

							static bool IsNodeOrDerivedFromNode(INamedTypeSymbol namedTypeSymbol) {

								if(namedTypeSymbol.Name == "Node") {

									return true;

								}

								INamedTypeSymbol? baseTypeSymbol = namedTypeSymbol.BaseType;

								if(baseTypeSymbol == null) {

									return false;

								}

								if(baseTypeSymbol.Name == "Node") {

									return true;

								}

								return IsNodeOrDerivedFromNode(baseTypeSymbol);

							}

							return compilation.GlobalNamespace
												.GetNamespaceMembers()
												.Where(namespaceSymbol =>
																namespaceSymbol.Name == "Godot")
												.SingleOrDefault()?
												.GetTypeMembers()
												.Where(namedTypeSymbol =>
																	!namedTypeSymbol.IsGenericType)
												.Where(namedTypeSymbol =>
																	namedTypeSymbol.BaseType != null)
												.Where(namedTypeSymbol =>
																	IsNodeOrDerivedFromNode(namedTypeSymbol))
												.Select(namedTypeSymbol =>
																new NormalNode(namedTypeSymbol.Name,
																				namedTypeSymbol.BaseType!.Name == "GodotObject" ?
																							"NodeBase" :
																							namedTypeSymbol.BaseType!.Name,
																				namedTypeSymbol.InstanceConstructors
																													.Any(constructorSymbol =>
																																	!constructorSymbol.Parameters
																																						.Any() &&
																																		constructorSymbol.DeclaredAccessibility ==
																																			Accessibility.Public),
																				[
																					.. namedTypeSymbol.GetMembers()
																										.OfType<IPropertySymbol>()
																										.Where(propertySymbol =>
																														propertySymbol.DeclaredAccessibility == Accessibility.Public)
																										.Where(propertySymbol =>
																														propertySymbol.SetMethod != null)
																										.Select(propertySymbol =>
																															(propertySymbol.Type.ToString(),
																																propertySymbol.Name))
																				],
																				[
																					.. namedTypeSymbol.GetMembers()
																										.OfType<IEventSymbol>()
																										.Select(eventSymbol =>
																															(eventSymbol.Type.ToString(),
																																eventSymbol.Name,
																																$"On{eventSymbol.Name}"))
																				])) ??
													[];
						});

		igiContext.RegisterSourceOutput(normalNodeClasses,
										(spContext, normalNodeClass) => {

											CompilationUnitSyntax compilationUnit =
												CompilationUnit()
													.AddUsings(
														UsingDirective(
															IdentifierName(
																"Godot"
															)
														)
													)
													.AddMembers(
														NamespaceDeclaration(
															IdentifierName(
																"DeclarativeGodot"
															)
														)
														.AddMembers(
															ClassDeclaration(
																normalNodeClass.DeclarativeName
															)
															.WithModifiers(
																TokenList(
																	((IEnumerable<SyntaxToken>) [
																		Token(
																			SyntaxKind.PublicKeyword
																		)
																	]).AppendIf(!normalNodeClass.HasAccessibleConstructor,
																				Token(
																					SyntaxKind.AbstractKeyword
																				))
																)
															)
															.WithBaseList(
																BaseList(
																	SeparatedList(
																		(IEnumerable<BaseTypeSyntax>) [
																			SimpleBaseType(
																				IdentifierName(
																					normalNodeClass.DeclarativeBaseTypeName
																				)
																			)
																		]
																	)
																)
															)
															.WithMembers(
																List(
																	normalNodeClass.Properties
																						.Concat(normalNodeClass.Events
																														.Select(@event =>
																																			(PropertyType: @event.EventType,
																																				PropertyName: @event.DeclarativePropertyName)))
																						.Select(property =>
																											PropertyDeclaration(
																												List<AttributeListSyntax>(),
																												TokenList(
																													Token(
																														SyntaxKind.PublicKeyword
																													),
																													Token(
																														SyntaxKind.VirtualKeyword
																													)
																												),
																												IdentifierName(
																													$"{property.PropertyType}?"
																												),
																												null,
																												Identifier(
																													property.PropertyName
																												),
																												AccessorList(
																													List(
																														[
																															AccessorDeclaration(
																																SyntaxKind.GetAccessorDeclaration
																															)
																															.WithModifiers(
																																TokenList(
																																	Token(
																																		SyntaxKind.ProtectedKeyword
																																	)
																																)
																															)
																															.WithSemicolonToken(
																																Token(
																																	SyntaxKind.SemicolonToken
																																)
																															),
																															AccessorDeclaration(
																																SyntaxKind.InitAccessorDeclaration
																															)
																															.WithSemicolonToken(
																																Token(
																																	SyntaxKind.SemicolonToken
																																)
																															)
																														]
																													)
																												)
																											))
																						.Cast<MemberDeclarationSyntax>()
																						.Append(MethodDeclaration(
																							List<AttributeListSyntax>(),
																							TokenList(
																								Token(
																									SyntaxKind.ProtectedKeyword
																								),
																								Token(
																									SyntaxKind.InternalKeyword
																								),
																								Token(
																									SyntaxKind.OverrideKeyword
																								)
																							),
																							PredefinedType(
																								Token(
																									SyntaxKind.VoidKeyword
																								)
																							),
																							null,
																							Identifier(
																								"SetNode"
																							),
																							null,
																							ParameterList(
																								SeparatedList(
																									[
																										Parameter(
																											List<AttributeListSyntax>(),
																											TokenList(),
																											IdentifierName(
																												"Node"
																											),
																											Identifier(
																												"node"
																											),
																											null
																										)
																									]
																								)
																							),
																							List<TypeParameterConstraintClauseSyntax>(),
																							Block(
																								(IEnumerable<StatementSyntax>) [
																									ExpressionStatement(
																										AssignmentExpression(
																											SyntaxKind.SimpleAssignmentExpression,
																											IdentifierName(
																												"Node"
																											),
																											CastExpression(
																												IdentifierName(
																													normalNodeClass.TypeName
																												),
																												IdentifierName(
																													"node"
																												)
																											)
																										)
																									),
																									ExpressionStatement(
																										InvocationExpression(
																											IdentifierName(
																												"SetProperties"
																											),
																											ArgumentList(
																												SeparatedList(
																													[
																														Argument(
																															IdentifierName(
																																"node"
																															)
																														)
																													]
																												)
																											)
																										)
																									),
																								]
																							),
																							null
																						))
																						.AppendIf(normalNodeClass.HasAccessibleConstructor,
																									MethodDeclaration(
																										List<AttributeListSyntax>(),
																										TokenList(
																											Token(
																												SyntaxKind.ProtectedKeyword
																											),
																											Token(
																												SyntaxKind.InternalKeyword
																											),
																											Token(
																												SyntaxKind.OverrideKeyword
																											)
																										),
																										IdentifierName(
																											"Node"
																										),
																										null,
																										Identifier(
																											"CreateNode"
																										),
																										null,
																										ParameterList(),
																										List<TypeParameterConstraintClauseSyntax>(),
																										Block(
																											(IEnumerable<StatementSyntax>) [
																												LocalDeclarationStatement(
																													VariableDeclaration(
																														IdentifierName(
																															normalNodeClass.TypeName
																														),
																														SeparatedList(
																															[
																																VariableDeclarator(
																																	Identifier(
																																		"node"
																																	),
																																	null,
																																	EqualsValueClause(
																																		ObjectCreationExpression(
																																			IdentifierName(
																																				normalNodeClass.TypeName
																																			),
																																			ArgumentList(),
																																			null
																																		)
																																	)
																																)
																															]
																														)
																													)
																												),
																												ExpressionStatement(
																													InvocationExpression(
																														IdentifierName(
																															"SetProperties"
																														),
																														ArgumentList(
																															SeparatedList(
																																[
																																	Argument(
																																		IdentifierName(
																																			"node"
																																		)
																																	)
																																]
																															)
																														)
																													)
																												),
																												ReturnStatement(
																													AssignmentExpression(
																														SyntaxKind.SimpleAssignmentExpression,
																														IdentifierName(
																															"Node"
																														),
																														IdentifierName(
																															"node"
																														)
																													)
																												)
																											]
																										),
																										null
																									))
																						.Append(MethodDeclaration(
																							List<AttributeListSyntax>(),
																							TokenList(
																								Token(
																									SyntaxKind.ProtectedKeyword
																								),
																								Token(
																									SyntaxKind.InternalKeyword
																								)
																							),
																							PredefinedType(
																								Token(
																									SyntaxKind.VoidKeyword
																								)
																							),
																							null,
																							Identifier(
																								"SetProperties"
																							),
																							null,
																							ParameterList(
																								SeparatedList(
																									[
																										Parameter(
																											List<AttributeListSyntax>(),
																											TokenList(),
																											IdentifierName(
																												normalNodeClass.TypeName
																											),
																											Identifier(
																												"node"
																											),
																											null
																										)
																									]
																								)
																							),
																							List<TypeParameterConstraintClauseSyntax>(),
																							Block(
																								((List<StatementSyntax>) [
																									.. normalNodeClass.Properties
																														.Select(property =>
																																		IfStatement(
																																			BinaryExpression(
																																				SyntaxKind.NotEqualsExpression,
																																				IdentifierName(
																																					property.PropertyName
																																				),
																																				LiteralExpression(
																																					SyntaxKind.NullLiteralExpression
																																				)
																																			),
																																			ExpressionStatement(
																																				AssignmentExpression(
																																					SyntaxKind.SimpleAssignmentExpression,
																																					MemberAccessExpression(
																																						SyntaxKind.SimpleMemberAccessExpression,
																																						IdentifierName(
																																							"node"
																																						),
																																						IdentifierName(
																																							property.PropertyName
																																						)
																																					),
																																					CastExpression(
																																						IdentifierName(
																																							property.PropertyType
																																						),
																																						IdentifierName(
																																							property.PropertyName
																																						)
																																					)
																																				)
																																			)
																																		))
																														.Cast<StatementSyntax>()
																														.Concat(normalNodeClass.Events
																																						.Select(@event =>
																																											IfStatement(
																																												BinaryExpression(
																																													SyntaxKind.NotEqualsExpression,
																																													IdentifierName(
																																														@event.DeclarativePropertyName
																																													),
																																													LiteralExpression(
																																														SyntaxKind.NullLiteralExpression
																																													)
																																												),
																																												ExpressionStatement(
																																													AssignmentExpression(
																																														SyntaxKind.AddAssignmentExpression,
																																														MemberAccessExpression(
																																															SyntaxKind.SimpleMemberAccessExpression,
																																															IdentifierName(
																																																"node"
																																															),
																																															IdentifierName(
																																																@event.EventName
																																															)
																																														),
																																														IdentifierName(
																																															@event.DeclarativePropertyName
																																														)
																																													)
																																												)
																																											)))
																								]).PrependIf(normalNodeClass.TypeName != "Node",
																												ExpressionStatement(
																														InvocationExpression(
																															MemberAccessExpression(
																																SyntaxKind.SimpleMemberAccessExpression,
																																BaseExpression(),
																																IdentifierName(
																																	"SetProperties"
																																)
																															),
																															ArgumentList(
																																SeparatedList(
																																	[
																																		Argument(
																																			IdentifierName(
																																				"node"
																																			)
																																		)
																																	]
																																)
																															)
																														)
																													))
																							),
																							null
																						))
																						.Append(MethodDeclaration(
																							List<AttributeListSyntax>(),
																							TokenList(
																								Token(
																									SyntaxKind.ProtectedKeyword
																								),
																								Token(
																									SyntaxKind.InternalKeyword
																								),
																								Token(
																									SyntaxKind.OverrideKeyword
																								)
																							),
																							IdentifierName(
																								"System.Type"
																							),
																							null,
																							Identifier(
																								"GetNodeType"
																							),
																							null,
																							ParameterList(),
																							List<TypeParameterConstraintClauseSyntax>(),
																							null,
																							ArrowExpressionClause(
																								TypeOfExpression(
																									IdentifierName(
																										normalNodeClass.TypeName
																									)
																								)
																							),
																							Token(
																								SyntaxKind.SemicolonToken
																							)
																						))
																						.Append(MethodDeclaration(
																							List<AttributeListSyntax>(),
																							TokenList(
																								Token(
																									SyntaxKind.PublicKeyword
																								),
																								Token(
																									SyntaxKind.OverrideKeyword
																								)
																							),
																							PredefinedType(
																								Token(
																									SyntaxKind.VoidKeyword
																								)
																							),
																							null,
																							Identifier(
																								"Dispose"
																							),
																							null,
																							ParameterList(),
																							List<TypeParameterConstraintClauseSyntax>(),
																							Block(
																								(List<StatementSyntax>) [
																									ExpressionStatement(
																										InvocationExpression(
																											MemberAccessExpression(
																												SyntaxKind.SimpleMemberAccessExpression,
																												BaseExpression(),
																												IdentifierName(
																													"Dispose"
																												)
																											)
																										)
																									),
																									LocalDeclarationStatement(
																										VariableDeclaration(
																											IdentifierName(
																												normalNodeClass.TypeName
																											),
																											SeparatedList(
																												[
																													VariableDeclarator(
																														Identifier(
																															"node"
																														),
																														null,
																														EqualsValueClause(
																															CastExpression(
																																IdentifierName(
																																	normalNodeClass.TypeName
																																),
																																PostfixUnaryExpression(
																																	SyntaxKind.SuppressNullableWarningExpression,
																																	IdentifierName(
																																		"Node"
																																	)
																																)
																															)
																														)
																													)
																												]
																											)
																										)
																									),
																									.. normalNodeClass.Events
																														.Select(@event =>
																																			IfStatement(
																																				BinaryExpression(
																																					SyntaxKind.NotEqualsExpression,
																																					IdentifierName(
																																						@event.DeclarativePropertyName
																																					),
																																					LiteralExpression(
																																						SyntaxKind.NullLiteralExpression
																																					)
																																				),
																																				ExpressionStatement(
																																					AssignmentExpression(
																																						SyntaxKind.SubtractAssignmentExpression,
																																						MemberAccessExpression(
																																							SyntaxKind.SimpleMemberAccessExpression,
																																							IdentifierName(
																																								"node"
																																							),
																																							IdentifierName(
																																								@event.EventName
																																							)
																																						),
																																						IdentifierName(
																																							@event.DeclarativePropertyName
																																						)
																																					)
																																				)
																																			))
																								]
																							),
																							null
																						))
																)
															)
															.NormalizeWhitespace()
														)
														.NormalizeWhitespace()
													)
													.WithLeadingTrivia(
														Trivia(
															NullableDirectiveTrivia(
																Token(
																	SyntaxKind.EnableKeyword
																),
																true
															)
														)
													)
													.NormalizeWhitespace();

											spContext.AddSource($"{normalNodeClass.DeclarativeName}.g.cs",
																compilationUnit.ToFullString());

										});

	}

	public record NormalNode(string TypeName,
								string BaseTypeName,
								bool HasAccessibleConstructor,
								List<(string PropertyType, string PropertyName)> Properties,
								List<(string EventType, string EventName, string DeclarativePropertyName)> Events) {

		public virtual string DeclarativeName =>
			$"D{TypeName}";

		public virtual string DeclarativeBaseTypeName =>
			$"D{BaseTypeName}";

	}

}