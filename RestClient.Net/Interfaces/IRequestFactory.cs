﻿namespace RestClient.Net.Interfaces
{
    public interface IRequestFactory
    {
        IRequest Create(string uri);
    }
}
