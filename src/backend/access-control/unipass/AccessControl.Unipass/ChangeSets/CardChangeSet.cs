using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Enums;

namespace AccessControl.Unipass.ChangeSets;

public class CardChangeSet : IChangeSet
{
    private int _personId = 0;
    private bool _assignCard = false;

    private Dictionary<string, object> _properties = [];

    private CardChangeSet() { }

    public static CardChangeSet Assign(int personId, int badgeNumber)
    {
        return new CardChangeSet()
        {
            _properties = new Dictionary<string, object>() { { "Operation", "Merge" }, { nameof(UnipassCardDto.Number), badgeNumber } },
            _personId = personId,
            _assignCard = true,
        };
    }

    public static CardChangeSet Revoke(int personId, int cardId)
    {
        return new CardChangeSet()
        {
            _properties = new Dictionary<string, object>() { { "Operation", "delete" }, { nameof(UnipassCardDto.Id), cardId } },
            _personId = personId,
            _assignCard = false,
        };
    }

    public CardChangeSet(TimeSpan validity)
    {
        _properties[nameof(UnipassCardDto.DaysValid)] = (int)Math.Ceiling(validity.TotalDays);
    }

    public async Task<ChangeSetDescription> BuildChangeSet(UnipassContext context)
    {
        Func<UnipassOperationResponse, UnipassOperationResponse>? transformer;

        if (_assignCard)
        {
            var person =
                await context.Api.GetPerson(_personId, context.CancellationToken)
                ?? throw new InvalidOperationException($"person with id {_personId} not found");

            var slotsInUse = person.Cards?.Select(x => x.Id) ?? [];

            int availableSlot = Enumerable.Range(1, 5).Except(slotsInUse).FirstOrDefault();

            if (availableSlot == 0)
            {
                //logger.LogWarning("No available card slots for person {PersonId}", personId);
                throw new InvalidOperationException($"No available slots for person {_personId}");
            }

            _properties[nameof(UnipassCardDto.Id)] = availableSlot;

            transformer = x =>
            {
                if (x.Success)
                    x.Id = availableSlot.ToString();

                return x;
            };
        }
        else
        {
            transformer = null;
        }

        List<Dictionary<string, object>> cardsData = [_properties];

        return new ChangeSetDescription(
            "Persons",
            UnipassOperation.Merge,
            new Dictionary<string, object>() { { "Id", _personId }, { "Card", cardsData } },
            transformer
        );
    }
}
