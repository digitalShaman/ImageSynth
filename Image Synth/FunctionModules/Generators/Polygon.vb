Public Class Polygon
    Inherits FunctionBase

    Private fAdd As Double = 0
    Private fIntensity As Double = 1


    Private iCorners As Int32 = 6
    Private iType As Int32 = 0
    Private xScale As Double = 10
    Private yScale As Double = 10
    Private xLocation As Double = 0
    Private yLocation As Double = 0
    Private fRotation As Double = 0
    Private fMin As Double = -1
    Private fMax As Double = 1

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Polygon"
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return "Erzeugt ein Polygon durch Anwendung des Voronoi Algorhithmus."
        End Get
    End Property


    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
        End Get
    End Property



    Private ltAllPoints As List(Of PointF)
    Private ptCenter As New PointF(0.5, 0.5)

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double


        If x = 0 AndAlso y = 0 Then
            'init
            ltAllPoints = New List(Of PointF)
            Dim fAlpha As Double = 0
            Dim fAlphaStep As Double = (Math.PI * 2) / iCorners

            ltAllPoints.Add(ptCenter)
            For i As Int32 = 1 To iCorners
                Dim xOffset As Double = Math.Sin(fAlpha)
                Dim yOffset As Double = Math.Cos(fAlpha)
                ltAllPoints.Add(New PointF(CSng(ptCenter.X + xOffset), CSng(ptCenter.Y + yOffset)))
                fAlpha += fAlphaStep
            Next
        End If

        'x = (x - 0.5) * xScale + xLocation
        'y = (y - 0.5) * yScale + yLocation

        x = x + xLocation
        y = y + yLocation


        Dim f As Double = Math.Sqrt((x - ptCenter.X) * (x - ptCenter.X) + (y - ptCenter.Y) * (y - ptCenter.Y))
        Dim fAng As Double = Math.Atan2(y - ptCenter.Y, x - ptCenter.X)
        Dim xRot As Double = Math.Sin(fAng - fRotation) * f
        Dim yRot As Double = Math.Cos(fAng - fRotation) * f
        x = ptCenter.X + xRot
        y = ptCenter.Y + yRot


        x = ptCenter.X + (x - ptCenter.X) * xScale
        y = ptCenter.Y + (y - ptCenter.Y) * yScale

        Dim f1 As Double = Double.MaxValue
        Dim f2 As Double = Double.MaxValue
        Dim fDistance As Double
        Select Case iType
            Case 0
                GetDistancesSpecial(ltAllPoints, x, y, f1, f2)
                fDistance = f2 - f1
            Case 1
                GetDistancesSpecial(ltAllPoints, x, y, f1, f2)
                fDistance = f1 - f2
            Case 2
                GetDistancesNormal(ltAllPoints, x, y, f1, f2)
                fDistance = f2 - f1
            Case 3
                GetDistancesNormal(ltAllPoints, x, y, f1, f2)
                fDistance = f1 - f2
            Case 4
                GetDistancesNormal(ltAllPoints, x, y, f1, f2)
                fDistance = 1 - f1
            Case 5
                GetDistancesNormal(ltAllPoints, x, y, f1, f2)
                fDistance = 1 - f2
            Case 6
                GetDistancesSpecial(ltAllPoints, x, y, f1, f2)
                fDistance = 1 - f2
            Case Else
                iType = 0
        End Select

        If fDistance < fMin Then
            fDistance = fMin
        ElseIf fDistance > fMax Then
            fDistance = fMax
        End If

        Return (fDistance + fAdd) * fIntensity
    End Function


    Private Sub GetDistancesNormal(ByVal Points As List(Of PointF), ByVal x As Double, ByVal y As Double, ByRef f1 As Double, ByRef f2 As Double)
        'voronoi normal
        f1 = Double.MaxValue
        f2 = Double.MaxValue
        Dim xDiff As Double
        Dim yDiff As Double
        Dim fDist As Double


        For Each pt As PointF In Points
            xDiff = (x - pt.X)
            yDiff = (y - pt.Y)
            fDist = xDiff * xDiff + yDiff * yDiff
            If fDist < f1 Then
                f2 = f1
                f1 = fDist
            ElseIf fDist < f2 Then
                f2 = fDist
            End If
        Next
        If f2 < Double.MaxValue Then
            f1 = Math.Sqrt(f1)
            f2 = Math.Sqrt(f2)
        End If
    End Sub

    Private Sub GetDistancesSpecial(ByVal Points As List(Of PointF), ByVal x As Double, ByVal y As Double, ByRef f1 As Double, ByRef f2 As Double)
        'f1=immer zum zentrum, f2 zum nächstgelegenen aussenpunkt
        Dim xDiff As Double
        Dim yDiff As Double
        Dim fDist As Double

        xDiff = (x - Points(0).X)
        yDiff = (y - Points(0).Y)
        f1 = xDiff * xDiff + yDiff * yDiff

        f2 = Double.MaxValue
        For Each pt As PointF In Points.Skip(1)
            xDiff = (x - pt.X)
            yDiff = (y - pt.Y)
            fDist = xDiff * xDiff + yDiff * yDiff
            If fDist < f2 Then
                f2 = fDist
            End If
        Next
        If f2 < Double.MaxValue Then
            f1 = Math.Sqrt(f1)
            f2 = Math.Sqrt(f2)
        End If
    End Sub

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

            tbl.Rows.Add(2, "Corners", 1, 6, iCorners)
            tbl.Rows.Add(3, "Type", 1, 4, iType)
            tbl.Rows.Add(4, "Scale X", 0.1, 10, xScale)
            tbl.Rows.Add(5, "Scale Y", 0.1, 10, yScale)
            tbl.Rows.Add(6, "Location X", 0.1, 0, xLocation)
            tbl.Rows.Add(7, "Location Y", 0.1, 0, yLocation)
            tbl.Rows.Add(8, "Rotation", 0.1, 0, fRotation)
            tbl.Rows.Add(9, "Minimum", 0.1, -1, fMin)
            tbl.Rows.Add(10, "Maximum", 0.1, 1, fMax)
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
                iCorners = CInt(Value)
            Case 3
                iType = CInt(Value)
            Case 4
                xScale = Value
            Case 5
                yScale = Value
            Case 6
                xLocation = Value
            Case 7
                yLocation = Value
            Case 8
                fRotation = Value
            Case 9
                fMin = Value
            Case 10
                fMax = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                iCorners += CInt(Value)
            Case 3
                iType += CInt(Value)
            Case 4
                xScale += Value
            Case 5
                yScale += Value
            Case 6
                xLocation += Value
            Case 7
                yLocation += Value
            Case 8
                fRotation += Value
            Case 9
                fMin += Value
            Case 10
                fMax += Value
        End Select
    End Sub
End Class
