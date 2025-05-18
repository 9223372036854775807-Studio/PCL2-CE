Imports System.IO.Ports
Imports System.Resources.ResXFileRef
Imports TagLib.Asf

Public Module ModSearch
    ''' <summary>
    ''' 加载全局搜索。
    ''' </summary>
    Public Sub SearchInit()
        FrmMain.PanSearch.Visibility = Visibility.Visible
        FrmMain.PanSearchMain.Opacity = 0

        FrmMain.PanSearchMainTransformScale.ScaleX = 0.93
        FrmMain.PanSearchMainTransformPos.Y = 20
        FrmMain.PanSearchMainTransformRotate.Angle = 0.6

        AniStart({
            AaColor(FrmMain.PanSearch, Grid.BackgroundProperty, New MyColor(90, 0, 0, 0) - FrmMain.PanSearch.Background, 200),
            AaOpacity(FrmMain.PanSearchMain, 1 - FrmMain.PanSearchMain.Opacity, 140, 0, New AniEaseInFluent(AniEasePower.Weak)),
            AaDouble(
            Sub(i)
                FrmMain.PanSearchMainTransformScale.ScaleX += i
                FrmMain.PanSearchMainTransformScale.ScaleY += i
            End Sub, 1 - FrmMain.PanSearchMainTransformScale.ScaleX, 180),
            AaDouble(Sub(i) FrmMain.PanSearchMainTransformPos.Y += i, 0 - FrmMain.PanSearchMainTransformPos.Y, 180, 0, New AniEaseInFluent(AniEasePower.Weak)),
            AaDouble(Sub(i) FrmMain.PanSearchMainTransformRotate.Angle += i, 0 - FrmMain.PanSearchMainTransformRotate.Angle, 180, 0, New AniEaseInoutFluent(AniEasePower.Weak))
        }, "Search Init")

    End Sub

    Public Sub SearchClose()

        AniStart({
            AaColor(FrmMain.PanSearch, Grid.BackgroundProperty, New MyColor(0, 0, 0, 0) - FrmMain.PanSearch.Background, 200, Ease:=New AniEaseOutFluent(AniEasePower.Weak)),
            AaOpacity(FrmMain.PanSearchMain, -FrmMain.PanSearchMain.Opacity, 140, 40, New AniEaseOutFluent(AniEasePower.Weak)),
            AaDouble(
            Sub(i)
                FrmMain.PanSearchMainTransformScale.ScaleX += i
                FrmMain.PanSearchMainTransformScale.ScaleY += i
            End Sub, 0.93 - FrmMain.PanSearchMainTransformScale.ScaleX, 180),
            AaDouble(Sub(i) FrmMain.PanSearchMainTransformPos.Y += i, 20 - FrmMain.PanSearchMainTransformPos.Y, 180, 0, New AniEaseOutFluent(AniEasePower.Weak)),
            AaDouble(Sub(i) FrmMain.PanSearchMainTransformRotate.Angle += i, 0.6 - FrmMain.PanSearchMainTransformRotate.Angle, 180, 0, New AniEaseInoutFluent(AniEasePower.Weak)),
            AaCode(Sub() FrmMain.PanSearch.Visibility = Visibility.Collapsed, , True)
        }, "Search Close")
    End Sub
End Module
