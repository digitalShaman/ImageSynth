Imports System.Globalization

Public Class Sinus
    Inherits FunctionBase

    Private xFrequency As Double = 1
    Private xPhaseShift As Double = 0
    Private yFrequency As Double = 1
    Private yPhaseShift As Double = 0
    Private fAdd As Double = 1
    Private fIntensity As Double = 0.5

    Overrides ReadOnly Property Name() As String
        Get
            Return "Sinus"
        End Get
    End Property

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
        End Get
    End Property


    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Gibt das Produkt eines Horizontal und eines Vertikal verlaufendes Sinus Signal aus.{0}{0}" &
                "Result = (( {0}" &
                "   Sin(x * xScale + xLocation) {0}" &
                "   * {0}" &
                "   Sin(y * yFrequency + yLocation) {0}" &
                ") + Add ) * Intensity", Environment.NewLine)
        End Get
    End Property

    Private Pi2 As Double = Math.PI * 2
    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        'neu 1.0.0.1:
        x -= 0.5
        y -= 0.5

        Return (Math.Sin((x * xFrequency + xPhaseShift) * Pi2) *
                Math.Sin((y * yFrequency + yPhaseShift) * Pi2) + fAdd) * fIntensity
    End Function


    Public Overrides Sub SetParameterValue(ByVal Index As Int32, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                xFrequency = Value
            Case 3
                yFrequency = Value
            Case 4
                xPhaseShift = Value
            Case 5
                yPhaseShift = Value
        End Select
    End Sub


    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                xFrequency += Value
            Case 3
                yFrequency += Value
            Case 4
                xPhaseShift += Value
            Case 5
                yPhaseShift += Value
        End Select
    End Sub

    Overrides ReadOnly Property ParameterInfos() As DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))
            tbl.Rows.Add(0, "Add", 0.1, 1, fAdd)
            tbl.Rows.Add(1, "Intensity", 0.1, 0.5, fIntensity)
            tbl.Rows.Add(2, "Scale X", 0.1, 1, xFrequency)
            tbl.Rows.Add(3, "Scale Y", 0.1, 1, yFrequency)
            tbl.Rows.Add(4, "Location X", 0.1, 0, xPhaseShift)
            tbl.Rows.Add(5, "Location Y", 0.1, 0, yPhaseShift)
            Return tbl
        End Get
    End Property







    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateSinus(int i, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
float M_PI2 =3.1415926535897932384626433832795 * 2.0;

float EvaluateSinus(int i, vec2 Location) {
   float x = Location.x - 0.5;
   float y = Location.y - 0.5;
   return (sin((x * Storage[i + 2] + Storage[i + 4]) * M_PI2) *
           sin((y * Storage[i + 3] + Storage[i + 5]) * M_PI2) + Storage[i + 0]) * Storage[i + 1];
};"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={xFrequency.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={yFrequency.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={xPhaseShift.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 5}]={yPhaseShift.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateSinus({OGLStorageIndex}, textureCoordinate);"

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
