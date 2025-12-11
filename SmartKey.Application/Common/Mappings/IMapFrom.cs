using AutoMapper;

namespace SmartKey.Application.Common.Mappings
{
    public interface IMapFrom<TEntity>
    {
        void Mapping(Profile profile)
        {
            profile.CreateMap(typeof(TEntity), GetType());

            profile.CreateMap(GetType(), typeof(TEntity));
        }
    }
}
