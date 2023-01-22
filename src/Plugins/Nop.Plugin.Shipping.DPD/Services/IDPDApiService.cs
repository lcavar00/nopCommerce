using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Core.Domain;
using Nop.Plugin.Shipping.DPD.Models;

namespace Nop.Plugin.Shipping.DPD.Services
{
    public interface IDPDApiService
    {
        Task<ParcelResponse> GenerateParcelsByOrderIdAsync(int orderId, Address pickupAddress);
        Task<ParcelResponse> GenerateParcelsAsync(Order order, Address pickupAddress);
        Task<PrintLabelResponse> PrintLabelsByOrderIdAsync(int orderId);
        Task<PrintLabelResponse> PrintLabelByShipmentRequestIdAsync(int shipmentRequestId);
        Task<PrintLabelResponse> PrintLabelAsync(ShipmentRequest shipmentRequest);
        Task<byte[]> GetParcelManifestsByOrderIdAsync(int orderId);
        Task<byte[]> GetParcelManifestAsync(Order order);
        Task<byte[]> GetParcelManifestByShipmentRequestIdAsync(int shipmentRequestId);
        Task<byte[]> GetParcelManifestAsync(ShipmentRequest shipmentRequest);
        Task<IEnumerable<ParcelStatusResponse>> GetParcelStatusesByOrderIdAsync(int orderId);
        Task<IEnumerable<ParcelStatusResponse>> GetParcelStatusesAsync(Order order);
        Task<IEnumerable<ParcelStatusResponse>> GetParcelStatusesAsync(IList<Shipment> shipments);
        Task<ParcelStatusResponse> GetParcelStatusByShipmentIdAsync(int shipmentId);
        Task<ParcelStatusResponse> GetParcelStatusAsync(Shipment shipment);
        Task<ParcelStatusResponse> GetParcelStatusByShipmentRequestIdAsync(int shipmentRequestId);
        Task<ParcelStatusResponse> GetParcelStatusAsync(ShipmentRequest shipmentRequest);
        Task<IList<DeleteOrCancelParcelResponse>> DeleteParcelsByOrderIdAsync(int orderId);
        Task<IList<DeleteOrCancelParcelResponse>> DeleteParcelsAsync(Order order);
        Task<DeleteOrCancelParcelResponse> DeleteParcelByShipmentRequestIdAsync(int shipmentRequestId);
        Task<DeleteOrCancelParcelResponse> DeleteParcelAsync(ShipmentRequest shipmentRequest);
        Task<DeleteOrCancelParcelResponse> CancelParcelsByOrderIdAsync(int orderId);
        Task<DeleteOrCancelParcelResponse> CancelParcelsAsync(Order order);
        Task<DeleteOrCancelParcelResponse> CancelParcelByShipmentIdAsync(int shipmentId);
        Task<DeleteOrCancelParcelResponse> CancelParcelAsync(Shipment shipment);
        Task<DeleteOrCancelParcelResponse> CancelParcelsAsync(IList<Shipment> shipments);
        Task<DeleteOrCancelParcelResponse> CancelParcelByShipmentRequestIdAsync(int shipmentRequestId);
        Task<DeleteOrCancelParcelResponse> CancelParcelAsync(ShipmentRequest shipmentRequest);
    }
}
