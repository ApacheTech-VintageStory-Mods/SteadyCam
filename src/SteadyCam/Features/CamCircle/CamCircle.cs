// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.SteadyCam.Features.CamCircle;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CamCircle : ClientModSystem
{
    private SystemCinematicCamera _cinematicCamera;

    private static string L(string path, params string[] args)
    {
        return LangEx.FeatureString("CamCircle", path, args);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        _cinematicCamera = IOC.Services.Resolve<SystemCinematicCamera>();
        var parsers = api.ChatCommands.Parsers;
        var camCommand = api.ChatCommands.Get("cam");

        var circleArgs = new ICommandArgumentParser[]
        {
            parsers.Float("radius"),
            parsers.OptionalInt("revolutions", defaultValue: 1),
            parsers.OptionalWordRange("direction", "left", "right"),
        };

        camCommand.BeginSubCommand("circle").WithDescription(L("Description")).WithArgs(circleArgs).HandleWith(args =>
        {
            var radius = (float)args.Parsers[0].GetValue();
            var revolutions = (int)args.Parsers[1].GetValue();
            var direction = (string)args.Parsers[2].GetValue();

            GenerateCircles(radius, revolutions, direction.IfNullOrEmpty("right"));

            return TextCommandResult.Success();
        });
    }

    public void GenerateCircles(float radius, int revolutions, string direction)
    {
        var circleVectors = direction.Equals("right")
            ? CircleVectors.RightMovingCircle
            : CircleVectors.LeftMovingCircle;

        var pos = ApiEx.ClientMain.EntityPlayer.Pos;

        for (var i = 0; i < revolutions; i++)
        {
            foreach (var v in circleVectors)
            {
                AddPoint(new(
                    pos.X + v.X * (double)radius,
                    pos.Y,
                    pos.Z + v.Z * (double)radius,
                    pos.Yaw,
                    pos.Pitch,
                    pos.Roll
                ));
            }
        }
        ClosePath();
        ApiEx.ClientMain.ShowChatMessage(L("CircleAdded"));
    }

    public void AddPoint(EntityPos pos)
    {
        var cameraPointsCount = _cinematicCamera.GetField<int>("cameraPointsCount");
        var cameraPoints = _cinematicCamera.GetField<CameraPoint[]>("cameraPoints");

        if (cameraPointsCount == 0)
        {
            _cinematicCamera.SetField("origin", pos.AsBlockPos);
        }
        var point = CameraPoint.FromEntityPos(pos);

        if (cameraPointsCount > 0)
        {
            var previousPoint = cameraPoints[cameraPointsCount - 1];
            var pointEquals = previousPoint?.CallMethod<bool>("PositionEquals", point) ?? false;
            if (pointEquals)
            {
                var x = point.GetField<double>("x");
                point.SetField("x", x + 0.0010000000474974513);
            }
        }

        cameraPoints[cameraPointsCount++] = point;
        _cinematicCamera.SetField("cameraPointsCount", cameraPointsCount);
        _cinematicCamera.SetField("cameraPoints", cameraPoints);

        if (cameraPointsCount > 1)
        {
            _cinematicCamera.CallMethod("FixYaw", cameraPointsCount);
        }
        _cinematicCamera.SetField("closedPath", false);
        _cinematicCamera.CallMethod("GenerateCameraPathModel");
    }

    public void ClosePath()
    {
        var cameraPointsCount = _cinematicCamera.GetField<int>("cameraPointsCount");
        var cameraPoints = _cinematicCamera.GetField<CameraPoint[]>("cameraPoints");

        cameraPoints[cameraPointsCount++] = cameraPoints[0];
        _cinematicCamera.SetField("cameraPointsCount", cameraPointsCount);
        _cinematicCamera.SetField("cameraPoints", cameraPoints);

        _cinematicCamera.CallMethod("FixYaw", cameraPointsCount);
        _cinematicCamera.SetField("closedPath", true);
        _cinematicCamera.CallMethod("GenerateCameraPathModel");
    }
}