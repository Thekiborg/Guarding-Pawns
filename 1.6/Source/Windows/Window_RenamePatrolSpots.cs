namespace Thek_GuardingPawns
{
	public class Window_RenamePatrolSpots : Window
	{
		private readonly GuardingP_PatrolSpots patrolSpot;
		private string label;

		private const float membersHeight = 35f;
		private const float vertPadding = 10f;
		private const float buttonWidthReduction = 15f;

		public override Vector2 InitialSize => new(280f, 175f);
		

		public Window_RenamePatrolSpots(GuardingP_PatrolSpots patrolSpot)
		{
			this.patrolSpot = patrolSpot;
			label = patrolSpot.Label;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Rect textRect = new (inRect)
			{
				height = Text.LineHeight + 10f
			};
			Widgets.Label(textRect, "Rename".Translate());
			Text.Font = GameFont.Small;


			Rect textFieldRect = new(0f, textRect.height, inRect.width, membersHeight);
			string newLabel = Widgets.TextField(textFieldRect, label);
			label = newLabel;


			Rect buttonRect = new(buttonWidthReduction,
								  inRect.height - textFieldRect.height - vertPadding,
								  inRect.width - (buttonWidthReduction * 2),
								  membersHeight);


			if (!Widgets.ButtonText(buttonRect, "OK"))
			{
				return;
			}
			patrolSpot.newLabel = label;


			Find.WindowStack.TryRemove(this);
		}
	}
}
