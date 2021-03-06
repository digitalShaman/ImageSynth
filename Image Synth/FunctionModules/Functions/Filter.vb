﻿Imports System.Globalization

Public Class Filter
    Inherits FunctionBase

    Private fAdd As Double = 0
    Private fIntensity As Double = 1

    Private fMin As Double = 0
    Private fMax As Double = 1
    Private fFilterSignal As Double = 1
    Private fFilteredSignal As Double = 1
    Private fDefaultLowSignal As Double = 0
    Private fDefaultHighSignal As Double = 0

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        If Double.IsNaN(fFilterSignal) Then
            fFilterSignal = fDefaultLowSignal
        End If
        If fFilterSignal < fMin Then
            Return (fDefaultLowSignal + fAdd) * fIntensity
        ElseIf fFilterSignal > fMax Then
            Return (fDefaultHighSignal + fAdd) * fIntensity
        Else
            Return (fFilteredSignal + fAdd) * fIntensity
        End If
    End Function

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Filter"
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Liegt das Filter Signal unter Minimum wird Default Low ausgegeben, liegt das Filter Signal über Maximum wird Default High ausgegeben, " &
                                 "sonst Signal." &
                                "{0}" &
                "Result = (({0}" &
                "   Wenn FilterSignal < Min {0}" &
                "       DefaultLow {0}" &
                "   Wenn FilterSignal > Max {0}" &
                "       DefaultHigh {0}" &
                "   sonst {0}" &
                "       Signal {0}" &
                ") + Add ) * Intensity" &
                "{0}" &
                "", Environment.NewLine)
        End Get
    End Property

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Function
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
            tbl.Rows.Add(2, "Minimum", 0.1, 0, fMin)
            tbl.Rows.Add(3, "Maximum", 0.1, 1, fMax)
            tbl.Rows.Add(4, "Filter Signal", 0.1, 1, fFilterSignal)
            tbl.Rows.Add(5, "Signal", 0.1, 1, fFilteredSignal)
            tbl.Rows.Add(6, "Default Low", 0.1, 0, fDefaultLowSignal)
            tbl.Rows.Add(7, "Default High", 0.1, 1, fDefaultHighSignal)
            Return tbl
        End Get
    End Property


    Public Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                fMin = Value
            Case 3
                fMax = Value
            Case 4
                fFilterSignal = Value
            Case 5
                fFilteredSignal = Value
            Case 6
                fDefaultLowSignal = Value
            Case 7
                fDefaultHighSignal = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                fMin += Value
            Case 3
                fMax += Value
            Case 4
                fFilterSignal += Value
            Case 5
                fFilteredSignal += Value
            Case 6
                fDefaultLowSignal += Value
            Case 7
                fDefaultHighSignal += Value
        End Select
    End Sub







    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateFilter(int i, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
float EvaluateFilter(int i, vec2 Location) {
	float fFilterSignal = Storage[i + 4];
	if (isnan(fFilterSignal)) {
		fFilterSignal = Storage[i + 6];
	}

	float fResult;
	if (fFilterSignal < Storage[i + 2]) {
		fResult = Storage[i + 6];
	} else if (fFilterSignal > Storage[i + 3]) {
		fResult = Storage[i + 7];
	} else {
		fResult = Storage[i + 5];
	}

    return (fResult + Storage[i + 0]) * Storage[i + 1];
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={fMin.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={fMax.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={fFilterSignal.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 5}]={fFilteredSignal.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 6}]={fDefaultLowSignal.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 7}]={fDefaultHighSignal.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateFilter({OGLStorageIndex}, textureCoordinate);"

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
