# Math

You can use [AsciiMath](http://asciimath.org) or [JLaTeXMath](https://github.com/opencollab/jlatexmath) notation within PlantUML:

```plantuml
@startuml
:<math>int_0^1f(x)dx</math>;
:<math>x^2+y_1+z_12^34</math>;
note right
Try also
<math>d/dxf(x)=lim_(h->0)(f(x+h)-f(x))/h</math>
<latex>P(y|\mathbf{x}) \mbox{ or } f(\mathbf{x})+\epsilon</latex>
end note
@enduml
```

or:

```plantuml
@startuml
Bob -> Alice : Can you solve: <math>ax^2+bx+c=0</math>
Alice --> Bob: <math>x = (-b+-sqrt(b^2-4ac))/(2a)</math>
@enduml
```
## Standalone diagram

You can also use ''@startmath''/''@endmath'' to create standalone [[http://asciimath.org|AsciiMath]] formula.

```plantuml
@startmath
f(t)=(a_0)/2 + sum_(n=1)^ooa_ncos((npit)/L)+sum_(n=1)^oo b_n\ sin((npit)/L)
@endmath
```

Or use ''@startlatex''/''@endlatex'' to create standalone [[https://github.com/opencollab/jlatexmath|JLaTeXMath]] formula. 

```plantuml
@startlatex
\sum_{i=0}^{n-1} (a_i + b_i^2)
@endlatex
```

## How is this working?
To draw those formulas, PlantUML uses two OpenSource projects: 

  * [AsciiMath](https://github.com/asciimath/asciimathml/tree/master/asciimath-based) that converts AsciiMath notation to LaTeX expression.
  * [JLatexMath](https://github.com/opencollab/jlatexmath) that displays mathematical formulas written in LaTeX. JLaTeXMath is the best Java library to display LaTeX code.

[ASCIIMathTeXImg.js](https://github.com/asciimath/asciimathml/blob/master/asciimath-based/ASCIIMathTeXImg.js) is small enough to be integrated into PlantUML standard distribution.

Since [JLatexMath](https://github.com/opencollab/jlatexmath) is bigger, you have to [[http://beta.plantuml.net/plantuml-jlatexmath.zip|download it]] separately, then unzip the 4 jar files (//batik-all-1.7.jar//, //jlatexmath-minimal-1.0.3.jar//, //jlm_cyrillic.jar// and //jlm_greek.jar//) in the same folder as PlantUML.jar. 
