Imports System.Globalization

Public Class Crossfade
    Inherits FunctionBase

    Private fAdd As Double = 0
    Private fIntensity As Double = 1

    Private fSwitch As Double = 0
    Private fSignal1 As Double = 0
    Private fSignal2 As Double = 0



    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        Dim fTemp As Double = fSwitch
        If fTemp < 0 Then
            fTemp = 0
        ElseIf fTemp > 1 Then
            fTemp = 1
        End If
        Dim fResult As Double = fSignal1 * (1 - fTemp) + fSignal2 * fTemp

        Return (fResult + fAdd) * fIntensity
    End Function

    Public Overrides ReadOnly Property Help As String
        Get
            Return "Das Ergebnis ist Signal 1 * (1 - Switch) + Signal 2 * Switch. Wobei Switch auf 0-1 beschränkt wird."
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Crossfade"
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

            tbl.Rows.Add(2, "Switch", 0.1, 0, fSwitch)
            tbl.Rows.Add(3, "Signal 1", 0.1, 0, fSignal1)
            tbl.Rows.Add(4, "Signal 2", 0.1, 0, fSignal2)
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
                fSwitch = Value
            Case 3
                fSignal1 = Value
            Case 4
                fSignal2 = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                fSwitch += Value
            Case 3
                fSignal1 += Value
            Case 4
                fSignal2 += Value
        End Select
    End Sub

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Function
        End Get
    End Property






    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateCrossfade(int i, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
float EvaluateCrossfade(int i, vec2 Location) {
	float fSwitch = Storage[i + 2];
	if (fSwitch < 0.0) {
		fSwitch = 0.0;
	} else if (fSwitch > 1.0) {
		fSwitch = 1.0;
	}

    float fResult = Storage[i + 3] * (1.0 - fSwitch) + Storage[i + 4] * fSwitch;
    return (fResult + Storage[i + 0]) * Storage[i + 1];
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={fSwitch.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={fSignal1.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={fSignal2.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateCrossfade({OGLStorageIndex}, textureCoordinate);"

        'Ergebnis an folge funktionen weitergeben
        For Each oChild As OutputTo In Outputs
            If Not (AnySolo AndAlso oChild.ToFunction.Type = FunctionType.ImageOutput) AndAlso oChild.ToFunction.OGLStorageIndex <> -1 Then
                'aber nur, wenn kein solo oder wenn solo dann aber nicht an imageoutput
                Dim oFunc As FunctionBase = CType(oChild.ToFunction, FunctionBase)
                sExecuteCode &= oFunc.GetOpenGlAddCode(oChild.ToPin, sTempVarName)
            End If
        Next

        Return sExecuteCode
    End Function

End Class
