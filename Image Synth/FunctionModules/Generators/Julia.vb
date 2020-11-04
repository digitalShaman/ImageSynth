Imports System.Globalization

Public Class Julia
    Inherits FunctionBase

    Private fAdd As Double = 0
    Private fIntensity As Double = 1

    Private fCValue As Double = 0.36
    Private xScale As Double = 1
    Private yScale As Double = 1
    Private xLocation As Double = 0
    Private yLocation As Double = 0

    Private Const MAX_ITERATIONS As Integer = 300


    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        x = (x - 0.5) * xScale + xLocation
        y = (y - 0.5) * yScale + yLocation

        Dim tmp As Int32 = CountInterations(x, y)
        Dim fResult As Double = tmp / MAX_ITERATIONS
        Return (fResult + fAdd) * fIntensity
    End Function

    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Erstellt eine Julia Menge." &
                "{0}" &
                "Es werden bis zu {1} Iterationen x = (x * x - y * y) + c bzw. y = (x * y + x * y) + c pro Punkt gerechnet solange " &
                "(x * x + y * y) < 4. {0}" &
                "Die Anzahl der Iterationen ergibt dann das Ergebnissignal nach:{0}" &
                "{0}" &
                "Result = (({0}" &
                "   Iterationen / {1} {0}" &
                ") + Add ) * Intensity {0}" &
                "", Environment.NewLine, MAX_ITERATIONS)

        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Julia"
        End Get
    End Property

    Public Overrides ReadOnly Property ParameterInfos As System.Data.DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))
            tbl.Rows.Add(0, "Add", 0.1, 0, fAdd)
            tbl.Rows.Add(1, "Intensity", 0.1, 1, fIntensity)
            tbl.Rows.Add(2, "C", 0.1, 0.36, fCValue)
            tbl.Rows.Add(3, "Scale X", 0.1, 1, xScale)
            tbl.Rows.Add(4, "Scale Y", 0.1, 1, yScale)
            tbl.Rows.Add(5, "Location X", 0.1, 0, xLocation)
            tbl.Rows.Add(6, "Location Y", 0.1, 0, yLocation)
            Return tbl
        End Get
    End Property

    Public Overloads Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                fCValue = Value
            Case 3
                xScale = Value
            Case 4
                yScale = Value
            Case 5
                xLocation = Value
            Case 6
                yLocation = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                fCValue += Value
            Case 3
                xScale += Value
            Case 4
                yScale += Value
            Case 5
                xLocation += Value
            Case 6
                yLocation += Value
        End Select
    End Sub

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
        End Get
    End Property



    Private Function CountInterations(ByVal x As Double, ByVal y As Double) As Int32
        Dim c As Int32 = 0
        While c < MAX_ITERATIONS + 1 AndAlso Not (x * x + y * y > 4)
            c += 1
            Dim yTemp As Double = x * y * 2
            x = x * x - y * y + fCValue
            y = yTemp + fCValue
        End While

        Return c - 1
    End Function








    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateJulia(int iModule, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
const int MAX_JULIA_ITERATIONS = 300;
int CountJuliaIterations(float x, float y, float fCValue) {
	int c = 0;
	float yTemp;
	while ( (c < MAX_JULIA_ITERATIONS + 1) && !(x * x + y * y > 4.0) ) {
            c++;
			yTemp = x * y * 2.0;
            x = x * x - y * y + fCValue;
            y = yTemp + fCValue;
	}
    return c - 1;
}
float EvaluateJulia(int iModule, vec2 Location) {
	Location.x = (Location.x - 0.5) * Storage[iModule + 3] + Storage[iModule + 5];
	Location.y = (Location.y - 0.5) * Storage[iModule + 4] + Storage[iModule + 6];


	float fResult = float(CountJuliaIterations(Location.x, Location.y, Storage[iModule + 2])) / float(MAX_JULIA_ITERATIONS);
    return (fResult + Storage[iModule + 0]) * Storage[iModule + 1];
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={fCValue.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={xScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={yScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 5}]={xLocation.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 6}]={yLocation.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateJulia({OGLStorageIndex}, textureCoordinate);"

        If Not Me.IsSolo Then
            'Ergebnis an folge funktionen weitergeben
            For Each oChild As OutputTo In Outputs
                If Not (AnySolo AndAlso oChild.ToFunction.Type = FunctionType.ImageOutput) AndAlso oChild.ToFunction.OGLStorageIndex <> -1 Then
                    'aber nur, wenn kein solo oder wenn solo dann aber nicht an imageoutput
                    Dim oFunc As FunctionBase = CType(oChild.ToFunction, FunctionBase)
                    sExecuteCode &= oFunc.GetOpenGlAddCode(oChild.ToPin, sTempVarName)
                End If
            Next
        End If

        Return sExecuteCode
    End Function
End Class
