//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineMusicServices.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class RankingAlbum
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public int Week { get; set; }
        public int Rank { get; set; }
        public int LastRank { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
    
        public virtual Album Album { get; set; }
    }
}
