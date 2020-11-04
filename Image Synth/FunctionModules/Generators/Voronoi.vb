Imports System.Globalization

Public Class Voronoi
    Inherits FunctionBase


    Private fAdd As Double = 0
    Private fIntensity As Double = 2
    Private iType As Int32 = 0
    Private xScale As Double = 1
    Private yScale As Double = 1
    Private xLocation As Double = 0
    Private yLocation As Double = 0

    'die beiden während der ausführung zu ändern macht keinen sinn
    Private cPoints As Int32 = 5
    Private iSeed As Int32 = 1

    Private arrPoints(,) As List(Of PointF)
    Private ltAllPoints As List(Of PointF)


    Private Sub AddIndexedPoint(pt As PointF)
        ltAllPoints.Add(pt)

        Dim iRowKey As Int32 = GetKey(pt.X)
        Dim iColKey As Int32 = GetKey(pt.Y)
        If Not arrPoints(iRowKey, iColKey).Contains(pt) Then
            arrPoints(iRowKey, iColKey).Add(pt)
        End If
    End Sub

    Private Function GetKey(ByVal f As Double) As Int32
        Return CInt(Math.Truncate(21 + f * 10))
    End Function


    Private cMisses As Int32
    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        If x = 0 AndAlso y = 0 Then
            'initialisieren
            GeneratePoints()

            cMisses = 0
        End If

        'neu 1.0.0.1
        x = (x - 0.5) * xScale + xLocation
        y = (y - 0.5) * yScale + yLocation

        'repeat:
        x = x - Math.Floor(x)
        y = y - Math.Floor(y)

        Dim f1 As Double = Double.MaxValue
        Dim f2 As Double = Double.MaxValue
        Dim pt1, pt2 As PointF

        If cPoints > 15 Then
            'über 15 punkte mit dic suchen ist schneller
            Dim iRowKey As Int32 = GetKey(x)
            Dim iColKey As Int32 = GetKey(y)
            Dim ltPointsQuickFiltered As New List(Of PointF)
            For xOffset As Int32 = -1 To +1
                For yOffset As Int32 = -1 To +1
                    ltPointsQuickFiltered.AddRange(arrPoints(iRowKey + xOffset, iColKey + yOffset))
                Next
            Next

            'GetDistances(ltPointsQuickFiltered, x, y, f1, f2, pt1, pt2)

            If f2 > 0.1 Then
                cMisses += 1
                'If cMisses Mod 100 = 0 Then
                '    Debug.Print(cMisses.ToString)
                'End If
                Dim pt12, pt22 As PointF
                GetDistances(ltAllPoints, x, y, f1, f2, pt12, pt22)
                If Not arrPoints(iRowKey, iColKey).Contains(pt12) Then
                    arrPoints(iRowKey, iColKey).Add(pt12)
                End If
                If Not arrPoints(iRowKey, iColKey).Contains(pt22) Then
                    arrPoints(iRowKey, iColKey).Add(pt22)
                End If
            End If

        Else
            '15 punkte oder weniger, besser gleich alle rechnen, da sonst zu viele misses
            GetDistances(ltAllPoints, x, y, f1, f2, pt1, pt2)
        End If


        Dim fDistance As Double
        Select Case iType
            Case 0
                fDistance = f1
            Case 1
                fDistance = 1 - f1
            Case 2
                fDistance = f2
            Case 3
                fDistance = 1 - f2
            Case 4
                fDistance = f2 - f1
            Case 5
                fDistance = f1 - f2
            Case 6
                fDistance = f1 + f2
            Case Else
                iType = 0
        End Select

        Return (fDistance + fAdd) * fIntensity
    End Function

    Private Sub GeneratePoints()
        Dim rnd As New Random(iSeed)
        ltAllPoints = New List(Of PointF)

        ClearArray()

        For i As Int32 = 1 To cPoints
            Dim pt As New PointF(CSng(rnd.NextDouble), CSng(rnd.NextDouble))
            For xOffset As Int32 = -1 To 1
                For yOffset As Int32 = -1 To 1
                    AddIndexedPoint(New PointF(pt.X + xOffset, pt.Y + yOffset))
                Next
            Next
        Next
    End Sub

    Private Sub ClearArray()
        ReDim arrPoints(50, 50)
        For x As Int32 = 0 To arrPoints.GetLength(0) - 1
            For y As Int32 = 0 To arrPoints.GetLength(1) - 1
                arrPoints(x, y) = New List(Of PointF)
            Next
        Next
    End Sub

    Private Sub GetDistances(ByVal Points As List(Of PointF), ByVal x As Double, ByVal y As Double, ByRef f1 As Double, ByRef f2 As Double,
                             ByRef pt1 As PointF, ByRef pt2 As PointF)
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
                pt2 = pt1
                f1 = fDist
                pt1 = pt
            ElseIf fDist < f2 Then
                f2 = fDist
                pt2 = pt
            End If
        Next
        If f2 < Double.MaxValue Then
            f1 = Math.Sqrt(f1)
            f2 = Math.Sqrt(f2)
        End If
    End Sub

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
        End Get
    End Property


    Public Overrides ReadOnly Property Name As String
        Get
            Return "Voronoi"
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Gibt ein Voronoi Diagramm aus. Die Anzahl an Punkten kann eingestellt werden, muss aber während " &
                "der Berechnung konstant bleiben. Die Punkte werden zufällig verteilt. {0}" &
                "{0}" &
                "f1 = Distanz zum nächsten Punkt{0}" &
                "f2 = Distanz zum zweitnächsten Punkt{0}" &
                "{0}" &
                "Result = (({0}" &
                "   Typ 0:  f1 {0}" &
                "   Typ 1:  1 - f1 {0}" &
                "   Typ 2:  f2 {0}" &
                "   Typ 3:  1 - f2 {0}" &
                "   Typ 4:  f2 - f1 {0}" &
                "   Typ 5:  f1 - f2 {0}" &
                "   Typ 6:  f1 + f2 {0}" &
                ") + Add ) * Intensity" &
                "{0}" &
                "Das Ergebnis ist in der Grundeinstellung seamless.", Environment.NewLine)
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
            tbl.Rows.Add(1, "Intensity", 0.1, 2, fIntensity)
            tbl.Rows.Add(2, "Number of Points", 1, 5, cPoints)
            tbl.Rows.Add(3, "Seed", 1, 1, iSeed)
            tbl.Rows.Add(4, "Type", 1, 0, iType)
            tbl.Rows.Add(5, "Scale X", 0.1, 1, xScale)
            tbl.Rows.Add(6, "Scale Y", 0.1, 1, yScale)
            tbl.Rows.Add(7, "Location X", 0.1, 1, xLocation)
            tbl.Rows.Add(8, "Location Y", 0.1, 1, yLocation)
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
                cPoints = CInt(Value)
            Case 3
                iSeed = CInt(Value)
            Case 4
                iType = CInt(Value)
            Case 5
                xScale = Value
            Case 6
                yScale = Value
            Case 7
                xLocation = Value
            Case 8
                yLocation = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                cPoints += CInt(Value)
            Case 3
                iSeed += CInt(Value)
            Case 4
                iType += CInt(Value)
            Case 5
                xScale += Value
            Case 6
                yScale += Value
            Case 7
                xLocation += Value
            Case 8
                yLocation += Value
        End Select
    End Sub





    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateVoronoi(int i, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
float EvaluateVoronoi(int i, vec2 Location) {
    float x = (Location.x - 0.5) * Storage[i + 5] + Storage[i + 7];
    float y = (Location.y - 0.5) * Storage[i + 6] + Storage[i + 8];
    x = x - floor(x);
    y = y - floor(y);
    float f1 = 1. / 0.;
    float f2 = 1. / 0.;

    vec2 pt[] = VoronoiPoints;
    int cPoints = int(Storage[i + 2]);
    int iStart = int(Storage[i + 3]);

	for (int xOffset = -1; xOffset < 2; xOffset++) {
		for (int yOffset = -1; yOffset < 2; yOffset++) {
			vec2 loc = vec2(x + float(xOffset), y + float(yOffset));
			for(int iPtCheck = iStart; iPtCheck < (iStart + cPoints); iPtCheck++)
			{
				float fDist = distance(loc,pt[iPtCheck]);
				if (fDist < f1) {
					f2 = f1;
					f1 = fDist;
				} else if(fDist < f2) {
					f2 = fDist;
				}
			};
		}
	}

    float fDistance;
    switch (int(Storage[i + 4])) {
        case 0:
            fDistance = f1;
            break;
        case 1:
            fDistance = 1.0 - f1;
            break;
        case 2:
            fDistance = f2;
            break;
        case 3:
            fDistance = 1.0 - f2;
            break;
        case 4:
            fDistance = f2 - f1;
            break;
        case 5:
            fDistance = f1 - f2;
            break;
        case 6:
            fDistance = f1 + f2;
            break;
    };
    return (fDistance + Storage[i + 0]) * Storage[i + 1];
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={cPoints.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]=0.0;")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={iType.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 5}]={xScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 6}]={yScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 7}]={xLocation.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 8}]={yLocation.ToString(sFormat, CultureInfo.InvariantCulture)};")


        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"

        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateVoronoi({OGLStorageIndex}, textureCoordinate);"

        'Ergebnis an folge funktionen weitergeben
        For Each oChild As OutputTo In Outputs
            If Not (AnySolo AndAlso oChild.ToFunction.Type = FunctionType.ImageOutput) AndAlso oChild.ToFunction.OGLStorageIndex <> -1 Then
                Dim oFunc As FunctionBase = CType(oChild.ToFunction, FunctionBase)
                sExecuteCode &= oFunc.GetOpenGlAddCode(oChild.ToPin, sTempVarName)
            End If
        Next

        Return sExecuteCode
    End Function


    Public ReadOnly Property GetPointCode() As String
        Get
            'spezial pt[] hack: arrays dynamischer größe können nicht an eine ogl funktion übergeben werden,
            'daher teilen sich alle voronois ein großes array und die func bekommt den startindex
            Dim rnd As New Random(iSeed)
            Dim sb As New Text.StringBuilder
            Dim sFormat As String = "0.0######"

            For i As Int32 = 1 To cPoints
                Dim pt As New PointF(CSng(rnd.NextDouble), CSng(rnd.NextDouble))
                Dim sX As String = CSng(pt.X).ToString(sFormat, CultureInfo.InvariantCulture)
                Dim sY As String = CSng(pt.Y).ToString(sFormat, CultureInfo.InvariantCulture)
                sb.Append($",vec2 ({sX},{sY})")
            Next

            Return sb.ToString
        End Get
    End Property


End Class




