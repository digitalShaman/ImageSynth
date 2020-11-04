Imports System.Globalization

Public Class RandomNoise
    Inherits FunctionBase

    Private fIntensity As Double = 1
    Private fAdd As Double = 0

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        Static rnd As Random = Nothing
        If x = 0 AndAlso y = 0 Then
            rnd = New Random(1)
        End If
        Return (rnd.NextDouble + fAdd) * fIntensity
    End Function

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Random Noise"
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Generiert zufällige Werte im Bereich von 0 und 1.{0}" &
                                "{0}" &
                "Result = (({0}" &
                "   rnd.NextDouble{0}" &
                ") + Add ) * Intensity {0}" &
                "", Environment.NewLine)
        End Get
    End Property


    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
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
            Return tbl
        End Get
    End Property


    Public Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
        End Select
    End Sub




End Class
