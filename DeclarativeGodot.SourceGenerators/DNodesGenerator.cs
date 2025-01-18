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

							static bool IsDerivedFromNode(INamedTypeSymbol namedTypeSymbol) {

								INamedTypeSymbol? baseTypeSymbol = namedTypeSymbol.BaseType;

								if(baseTypeSymbol == null) {

									return false;

								}

								if(baseTypeSymbol.Name == "Node") {

									return true;

								}

								return IsDerivedFromNode(baseTypeSymbol);

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
																	IsDerivedFromNode(namedTypeSymbol))
												.Select(namedTypeSymbol =>
																new NormalNode(namedTypeSymbol.Name,
																				namedTypeSymbol.BaseType!.Name,
																				namedTypeSymbol.InstanceConstructors
																													.Any(constructorSymbol =>
																																	!constructorSymbol.Parameters
																																						.Any() &&
																																		constructorSymbol.DeclaredAccessibility ==
																																			Accessibility.Public),
																				namedTypeSymbol.GetMembers()
																										.OfType<IPropertySymbol>()
																										.Where(propertySymbol =>
																															propertySymbol.DeclaredAccessibility == Accessibility.Public)
																										.Where(propertySymbol =>
																															propertySymbol.SetMethod != null)
																										.Select(propertySymbol =>
																															(propertySymbol.Type.ToString(), propertySymbol.Name))
																										.ToList())) ??
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
																	new List<SyntaxToken>() {
																		Token(
																			SyntaxKind.PublicKeyword
																		)
																	}.AppendIf(!normalNodeClass.HasAccessibleConstructor,
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
																													property.PropertyType
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
																										null,
																										ArrowExpressionClause(
																											ObjectCreationExpression(
																												IdentifierName(
																													normalNodeClass.Name
																												),
																												null,
																												InitializerExpression(
																													SyntaxKind.ObjectInitializerExpression,
																													SeparatedList<ExpressionSyntax>(
																														normalNodeClass.Properties
																																			.Select(property =>
																																								property.PropertyName)
																																			.Select(propertyName =>
																																								AssignmentExpression(
																																									SyntaxKind.SimpleAssignmentExpression,
																																									IdentifierName(
																																										propertyName
																																									),
																																									IdentifierName(
																																										propertyName
																																									)
																																								))
																													)
																												)
																											)
																										),
																										Token(
																											SyntaxKind.SemicolonToken
																										)
																									))
																)
															)
															.NormalizeWhitespace()
														)
														.NormalizeWhitespace()
													)
													.NormalizeWhitespace();

											spContext.AddSource($"{normalNodeClass.DeclarativeName}.g.cs",
																compilationUnit.ToFullString());

										});

	}

	public record NormalNode(string Name,
								string BaseTypeName,
								bool HasAccessibleConstructor,
								List<(string PropertyType, string PropertyName)> Properties) {

		public virtual string DeclarativeName =>
			$"D{Name}";

		public virtual string DeclarativeBaseTypeName =>
			$"D{BaseTypeName}";

	}

}