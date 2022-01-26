using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using MongoDB.Bson;

namespace BacklogApp.Tests
{
    internal class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() => new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new MongoCustomization())) { }
    }

    internal class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] objects)
            : base(new AutoMoqDataAttribute(), objects) { }
    }

    public class MongoCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(ObjectId.GenerateNewId);
        }
    }
}
