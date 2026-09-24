namespace ClauOS.Models
{
    public class TicketItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🎫";
        public string Description { get; set; } = string.Empty;
        public bool IsRedeemed { get; set; }
        public DateTime? RedeemedAt { get; set; }

        public string RedeemedAtText => RedeemedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
    }
}
