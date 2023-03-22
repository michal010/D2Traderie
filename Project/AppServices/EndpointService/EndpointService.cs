using D2Traderie.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointService
    {
        public EndpointService()
        {

        }

        public string GetItemsEndpoint(int page)
        {
            IEndpoint baseEndpoint = new Endpoint();
            IEndpoint itemsEndpointDecorator = new EndpointItemsDecorator(baseEndpoint);
            IEndpoint questionMarkDecorator = new EndpointQuestionMarkDecorator(itemsEndpointDecorator);
            IEndpoint propertiesDecorator = new EndpointItemPropertiesDecorator(questionMarkDecorator);
            IEndpoint andEndpointDecorator = new EndpointAndDecorator(propertiesDecorator);
            IEndpoint pageEndpointDecorator = new EndpointPageDecorator(andEndpointDecorator, page);
            return pageEndpointDecorator.GetEndpointURL();
        }

        public string GetTagsEndpoint()
        {
            IEndpoint baseEndpoint = new Endpoint();
            IEndpoint tagsEndpointDecorator = new EndpointTagsDecorator(baseEndpoint);
            IEndpoint itemsEndpointDecorator = new EndpointItemsDecorator(tagsEndpointDecorator);
            return itemsEndpointDecorator.GetEndpointURL();
        }

        public string GetListingEndpoint(uint itemID, int page)
        {
            IEndpoint baseEndpoint = new Endpoint();
            IEndpoint listingEndpoint = new EndpointListingsDecorator(baseEndpoint);
            IEndpoint itemEndpointDecorator = new EndpointItemDecorator(listingEndpoint, itemID.ToString());
            IEndpoint andEndpointDecorator = new EndpointAndDecorator(itemEndpointDecorator);
            //Property decorator
            IEndpoint propertyDecoratorPlatform = new EndpointPropertyDecorator(andEndpointDecorator, "prop_Platform", "PC");
            IEndpoint andEndpointDecorator2 = new EndpointAndDecorator(propertyDecoratorPlatform);
            IEndpoint propertyDecoratorMode = new EndpointPropertyDecorator(andEndpointDecorator2, "prop_Mode", "softcore");
            IEndpoint andEndpointDecorator3 = new EndpointAndDecorator(propertyDecoratorMode);
            IEndpoint propertyDecoratorLadder = new EndpointPropertyDecorator(andEndpointDecorator3, "prop_Ladder", "true");
            IEndpoint andEndpointDecorator4 = new EndpointAndDecorator(propertyDecoratorLadder);
            IEndpoint propertyDecoratorOffers = new EndpointPropertyDecorator(andEndpointDecorator4, "makeOffer", "false");
            IEndpoint andEndpointDecorator5 = new EndpointAndDecorator(propertyDecoratorOffers);
            IEndpoint propertyDecoratorEthereal = new EndpointPropertyDecorator(andEndpointDecorator5, "prop_Ethereal", "false");
            //end of property decorators... has to be done better
            IEndpoint andEndpointDecorator6 = new EndpointAndDecorator(propertyDecoratorEthereal);
            IEndpoint pageEndpointDecorator = new EndpointPageDecorator(andEndpointDecorator6, page);
            return pageEndpointDecorator.GetEndpointURL();
        }


    }
}
