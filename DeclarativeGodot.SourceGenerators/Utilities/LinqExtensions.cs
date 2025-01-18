using System.Collections.Generic;
using System.Linq;

namespace DeclarativeGodot.SourceGenerators.Utilities;

public static class LinqExtensions {

	public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable,
												bool condition,
												T value) =>
		condition ?
			enumerable.Append(value) :
			enumerable;

}